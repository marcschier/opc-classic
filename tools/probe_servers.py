#!/usr/bin/env python3
"""
Probe every Opc.Classic MCP tool over stdio JSON-RPC and emit JSON results.

The script starts mcp/Opc.Classic.Mcp in stdio mode, discovers exposed tools via
`tools/list`, then sends a best-effort probe call for each tool. It is intended
for live DA/HDA/AE test servers identified by CLSID, with DA/HDA/AE-specific
CLSID overrides available when those interfaces are implemented by different
COM classes.
"""

from __future__ import annotations

import argparse
import json
import os
import queue
import shutil
import subprocess
import sys
import threading
import time
from dataclasses import dataclass
from typing import Any, Callable, Optional


ProbeArgs = Callable[["ProbeRunner"], dict[str, Any]]
ProbeAfter = Callable[["ProbeRunner", Any], None]


@dataclass(frozen=True)
class ProbeSpec:
    name: str
    args: ProbeArgs
    after: Optional[ProbeAfter] = None


class McpClient:
    """Minimal JSON-RPC-over-stdio client for the MCP server."""

    def __init__(self, proc: subprocess.Popen[bytes], timeout_seconds: float) -> None:
        self._proc = proc
        self._timeout_seconds = timeout_seconds
        self._stdin = proc.stdin
        self._stdout = proc.stdout
        if self._stdin is None or self._stdout is None:
            raise RuntimeError("MCP server stdio pipes were not created.")

        self._lock = threading.Lock()
        self._next_id = 1
        self._responses: queue.Queue[Any] = queue.Queue()
        self._pending: dict[int, dict[str, Any]] = {}
        self._stdout_thread = threading.Thread(target=self._read_stdout, daemon=True)
        self._stderr_thread = threading.Thread(target=self._drain_stderr, daemon=True)
        self._stdout_thread.start()
        self._stderr_thread.start()

    def _drain_stderr(self) -> None:
        assert self._proc.stderr is not None
        for line in iter(self._proc.stderr.readline, b""):
            sys.stderr.write("[mcp.stderr] " + line.decode("utf-8", errors="replace"))

    def _read_stdout(self) -> None:
        assert self._stdout is not None
        for line in iter(self._stdout.readline, b""):
            try:
                self._responses.put(json.loads(line.decode("utf-8")))
            except Exception as ex:
                self._responses.put(ex)
        self._responses.put(EOFError("MCP server closed stdout."))

    def _send(self, payload: dict[str, Any]) -> None:
        encoded = (json.dumps(payload, separators=(",", ":")) + "\n").encode("utf-8")
        with self._lock:
            assert self._stdin is not None
            self._stdin.write(encoded)
            self._stdin.flush()

    def initialize(self) -> None:
        self.request("initialize", {
            "protocolVersion": "2025-03-26",
            "capabilities": {},
            "clientInfo": {"name": "probe_servers.py", "version": "1.0"},
        })
        self._send({"jsonrpc": "2.0", "method": "notifications/initialized", "params": {}})

    def request(self, method: str, params: Optional[dict[str, Any]] = None) -> Any:
        with self._lock:
            req_id = self._next_id
            self._next_id += 1
        self._send({"jsonrpc": "2.0", "id": req_id, "method": method, "params": params or {}})

        deadline = time.monotonic() + self._timeout_seconds
        while True:
            if req_id in self._pending:
                msg = self._pending.pop(req_id)
                return self._result_or_raise(method, msg)

            remaining = deadline - time.monotonic()
            if remaining <= 0:
                raise TimeoutError(f"{method} timed out after {self._timeout_seconds:g}s")

            try:
                msg = self._responses.get(timeout=remaining)
            except queue.Empty as ex:
                raise TimeoutError(f"{method} timed out after {self._timeout_seconds:g}s") from ex

            if isinstance(msg, BaseException):
                raise msg
            if not isinstance(msg, dict):
                continue
            msg_id = msg.get("id")
            if msg_id == req_id:
                return self._result_or_raise(method, msg)
            if isinstance(msg_id, int):
                self._pending[msg_id] = msg

    @staticmethod
    def _result_or_raise(method: str, msg: dict[str, Any]) -> Any:
        if "error" in msg:
            raise RuntimeError(f"{method} failed: {msg['error']}")
        return msg.get("result")

    def list_tools(self) -> list[dict[str, Any]]:
        result = self.request("tools/list")
        tools = result.get("tools") if isinstance(result, dict) else None
        return tools if isinstance(tools, list) else []

    def call_tool(self, name: str, arguments: dict[str, Any]) -> Any:
        envelope = self.request("tools/call", {"name": name, "arguments": arguments})
        is_error = bool(envelope.get("isError")) if isinstance(envelope, dict) else False
        value = unwrap_tool_result(envelope)
        if is_error:
            raise RuntimeError(first_line(value if isinstance(value, str) else json.dumps(value, default=str)))
        return value


def unwrap_tool_result(envelope: Any) -> Any:
    content = envelope.get("content") if isinstance(envelope, dict) else None
    if isinstance(content, list) and content:
        first = content[0]
        if isinstance(first, dict) and first.get("type") == "text":
            text = first.get("text") or ""
            try:
                return json.loads(text)
            except json.JSONDecodeError:
                return text
    return envelope


def launch_server(repo_root: str) -> subprocess.Popen[bytes]:
    dotnet = shutil.which("dotnet")
    if dotnet is None:
        raise RuntimeError("`dotnet` not found in PATH. Install the .NET 10 SDK.")
    csproj = os.path.join(repo_root, "mcp", "Opc.Classic.Mcp", "Opc.Classic.Mcp.csproj")
    if not os.path.exists(csproj):
        raise RuntimeError(f"MCP project not found at {csproj}.")
    return subprocess.Popen(
        [dotnet, "run", "--project", csproj, "--no-build", "-c", "Debug"],
        stdin=subprocess.PIPE,
        stdout=subprocess.PIPE,
        stderr=subprocess.PIPE,
        cwd=repo_root,
    )


class ProbeRunner:
    def __init__(self, args: argparse.Namespace, client: McpClient) -> None:
        self.args = args
        self.client = client
        self.results: list[dict[str, Any]] = []
        self.session_id = ""
        self.da_group_handle = 0
        self.da_item_handles: list[int] = []
        self.da_subscription_id = ""
        self.hda_item_handles: list[int] = []
        self.hda_aggregate = "Average"
        self.ae_category = 0
        self.ae_attribute_ids: list[int] = []
        self.ae_subscription_id = ""
        self.batch_enum_set_id = 0
        self.command_invocation_id = ""
        self.xmlda_subscription_handle = ""

    def run(self, tools: list[dict[str, Any]]) -> list[dict[str, Any]]:
        exposed = [tool.get("name") for tool in tools if isinstance(tool.get("name"), str)]
        exposed_set = set(exposed)
        covered: list[str] = []

        for spec in probe_specs():
            if spec.name in exposed_set:
                covered.append(spec.name)
                self.probe(spec)

        known = set(covered)
        schema_by_name = {tool.get("name"): tool.get("inputSchema") for tool in tools if isinstance(tool, dict)}
        for name in exposed:
            if name not in known:
                self.probe(ProbeSpec(name, lambda runner, tool_name=name: runner.auto_args(schema_by_name.get(tool_name))))

        return self.results

    def probe(self, spec: ProbeSpec) -> None:
        try:
            arguments = spec.args(self)
        except Exception as ex:
            arguments = {}
            self.results.append(make_result(spec.name, arguments, False, first_line(str(ex)), ""))
            return

        try:
            value = self.client.call_tool(spec.name, arguments)
            self.results.append(make_result(spec.name, arguments, True, None, summarize(value)))
            if spec.after is not None:
                spec.after(self, value)
        except Exception as ex:
            self.results.append(make_result(spec.name, arguments, False, first_line(str(ex)), ""))

    def dcom_args(self, kind: str, include_sso: bool = False) -> dict[str, Any]:
        result: dict[str, Any] = {"sessionId": self.session_id, "host": self.args.host}
        prog_id = getattr(self.args, f"{kind}_progid") or self.args.progid
        clsid = getattr(self.args, f"{kind}_clsid") or self.args.clsid
        connection_string = getattr(self.args, f"{kind}_connection_string") or self.args.connection_string
        if prog_id:
            result["progId"] = prog_id
        if clsid:
            result["clsid"] = clsid
        if self.args.username:
            result["username"] = self.args.username
        if self.args.password is not None:
            result["password"] = self.args.password
        if self.args.use_kerberos:
            result["useKerberos"] = True
        if connection_string:
            result["connectionString"] = connection_string
        if include_sso:
            result["useSso"] = self.args.use_sso
        return result

    def hda_handles_or_items(self) -> dict[str, Any]:
        if self.hda_item_handles:
            return {"serverHandles": self.hda_item_handles}
        return {"itemIds": [self.args.hda_item]}

    def first_hda_handle(self) -> int:
        return self.hda_item_handles[0] if self.hda_item_handles else 0

    def dx_connection(self) -> dict[str, Any]:
        return {
            "name": self.args.dx_connection_name,
            "description": "MCP probe connection",
            "itemPath": "",
            "itemName": self.args.da_bucket_int_item,
            "version": "probe",
            "browsePaths": [],
            "keyword": "probe",
            "defaultSourceItemConnected": False,
            "defaultTargetItemConnected": False,
            "targetItemPath": "",
            "targetItemName": self.args.da_bucket_int_item,
            "sourceServerName": self.args.dx_source_name,
            "sourceItemPath": "",
            "sourceItemName": self.args.da_bucket_int_item,
            "sourceItemQueueSize": 1,
            "updateRateMilliseconds": 1000,
            "deadbandPercent": 0,
            "vendorData": "probe",
            "mask": 0,
        }

    def dx_source(self) -> dict[str, Any]:
        return {
            "name": self.args.dx_source_name,
            "serverUrl": f"opcda://{self.args.host}/{self.args.progid or self.args.clsid or 'probe'}",
            "description": "MCP probe source",
            "serverType": "OPC DA",
            "itemPath": "",
            "itemName": self.args.da_bucket_int_item,
            "version": "probe",
            "defaultConnected": False,
            "mask": 0,
            "reserved": 0,
        }

    def auto_args(self, schema: Any) -> dict[str, Any]:
        arguments: dict[str, Any] = {}
        props = schema.get("properties") if isinstance(schema, dict) else None
        if not isinstance(props, dict):
            return {"sessionId": self.session_id} if self.session_id else {}
        for name, prop in props.items():
            if name == "sessionId":
                arguments[name] = self.session_id
            elif name == "host":
                arguments[name] = self.args.host
            elif name == "clsid":
                arguments[name] = self.args.clsid
            elif name == "progId":
                arguments[name] = self.args.progid
            elif name == "username":
                arguments[name] = self.args.username
            elif name == "password":
                arguments[name] = self.args.password or ""
            elif name in ("itemIds", "itemNames"):
                arguments[name] = [self.args.da_read_item]
            elif name.endswith("Handle"):
                arguments[name] = 0
            elif name.endswith("Handles"):
                arguments[name] = []
            elif isinstance(prop, dict) and name in (schema.get("required") or []):
                arguments[name] = default_for_schema(prop)
        return {key: value for key, value in arguments.items() if value is not None}


def probe_specs() -> list[ProbeSpec]:
    return [
        ProbeSpec("opcclassic.session.create", lambda r: {}, after_session_create),
        ProbeSpec("opcclassic.session.list", lambda r: {}),
        ProbeSpec("opcclassic.discovery.enumerate_servers", lambda r: {"host": r.args.host}),

        ProbeSpec("opcclassic.da.connect", lambda r: r.dcom_args("da", include_sso=True)),
        ProbeSpec("opcclassic.da.get_status", sid),
        ProbeSpec("opcclassic.da.browse", lambda r: {**sid(r), "itemId": "", "browseFilter": "all"}),
        ProbeSpec("opcclassic.da.browse", lambda r: {**sid(r), "itemId": r.args.da_browse_branch, "browseFilter": "leaf"}),
        ProbeSpec("opcclassic.da.get_properties", lambda r: {**sid(r), "itemIds": [r.args.da_read_item], "returnValues": True}),
        ProbeSpec("opcclassic.da.read_items_by_id", lambda r: {**sid(r), "itemIds": [r.args.da_read_item], "maxAges": [0]}),
        ProbeSpec("opcclassic.da.add_group", lambda r: {**sid(r), "name": r.args.da_group_name, "active": True, "updateRateMs": 500}, after_da_add_group),
        ProbeSpec("opcclassic.da.add_items", lambda r: {**sid(r), "groupHandle": r.da_group_handle, "itemIds": [r.args.da_bucket_int_item, r.args.da_bucket_string_item], "clientHandles": [1, 2], "active": True}, after_da_add_items),
        ProbeSpec("opcclassic.da.read_sync", lambda r: {**sid(r), "groupHandle": r.da_group_handle, "serverHandles": r.da_item_handles, "fromCache": False}),
        ProbeSpec("opcclassic.da.write_sync", lambda r: {**sid(r), "groupHandle": r.da_group_handle, "serverHandles": r.da_item_handles, "values": [r.args.da_write_int, r.args.da_write_string][:len(r.da_item_handles)]}),
        ProbeSpec("opcclassic.da.subscribe", lambda r: {**sid(r), "groupHandle": r.da_group_handle, "fromCache": True}, after_subscription("da_subscription_id")),
        ProbeSpec("opcclassic.da.poll_subscription", lambda r: {**sid(r), "subscriptionId": r.da_subscription_id, "maxNotifications": 0}),
        ProbeSpec("opcclassic.da.get_error_string", lambda r: {**sid(r), "hresult": 0, "localeId": 0}),

        ProbeSpec("opcclassic.security.is_available_nt", sid),
        ProbeSpec("opcclassic.security.is_available_private", sid),
        ProbeSpec("opcclassic.security.logon", lambda r: {**sid(r), "username": r.args.security_username, "password": r.args.security_password}),
        ProbeSpec("opcclassic.security.logoff", sid),
        ProbeSpec("opcclassic.cpx.get_type_system", lambda r: {**sid(r), "typeSystemId": "OPCBinary"}),
        ProbeSpec("opcclassic.cpx.get_complex_type", lambda r: {**sid(r), "itemId": r.args.da_read_item}),
        ProbeSpec("opcclassic.cpx.get_dictionary", lambda r: {**sid(r), "dictionaryId": r.args.cpx_dictionary_id}),

        ProbeSpec("opcclassic.da.remove_group", lambda r: {**sid(r), "groupHandle": r.da_group_handle, "force": True}),
        ProbeSpec("opcclassic.da.disconnect", sid),

        ProbeSpec("opcclassic.hda.connect", lambda r: r.dcom_args("hda")),
        ProbeSpec("opcclassic.hda.get_status", sid),
        ProbeSpec("opcclassic.hda.browse", lambda r: {**sid(r), "itemIdPrefix": "", "browseType": "leaf"}),
        ProbeSpec("opcclassic.hda.validate_items", lambda r: {**sid(r), "itemIds": [r.args.hda_item]}),
        ProbeSpec("opcclassic.hda.get_item_handles", lambda r: {**sid(r), "itemIds": [r.args.hda_item], "clientHandles": [1]}, after_hda_item_handles),
        ProbeSpec("opcclassic.hda.read_raw", lambda r: {**sid(r), "startTime": r.args.hda_start, "endTime": r.args.hda_end, "maxValuesPerItem": 10, "includeBounds": False, **r.hda_handles_or_items()}),
        ProbeSpec("opcclassic.hda.read_processed", lambda r: {**sid(r), "startTime": r.args.hda_start, "endTime": r.args.hda_end, "resampleIntervalSeconds": 60, "aggregate": r.hda_aggregate, **r.hda_handles_or_items()}),
        ProbeSpec("opcclassic.hda.read_at_time", lambda r: {**sid(r), "timestamps": [r.args.hda_at_time], **r.hda_handles_or_items()}),
        ProbeSpec("opcclassic.hda.read_modified", lambda r: {**sid(r), "startTime": r.args.hda_start, "endTime": r.args.hda_end, "maxValuesPerItem": 10, **r.hda_handles_or_items()}),
        ProbeSpec("opcclassic.hda.read_attribute", lambda r: {**sid(r), "serverHandle": r.first_hda_handle(), "attributeIds": [1, 2], "startTime": r.args.hda_start, "endTime": r.args.hda_end}),
        ProbeSpec("opcclassic.hda.read_annotations", lambda r: {**sid(r), "startTime": r.args.hda_start, "endTime": r.args.hda_end, **r.hda_handles_or_items()}),
        ProbeSpec("opcclassic.hda.get_aggregates", sid, after_hda_aggregates),
        ProbeSpec("opcclassic.hda.insert_data", lambda r: {**sid(r), "serverHandles": [], "timestamps": [], "values": [], "qualities": []}),
        ProbeSpec("opcclassic.hda.replace_data", lambda r: {**sid(r), "serverHandles": [], "timestamps": [], "values": [], "qualities": []}),
        ProbeSpec("opcclassic.hda.insert_replace_data", lambda r: {**sid(r), "serverHandles": [], "timestamps": [], "values": [], "qualities": []}),
        ProbeSpec("opcclassic.hda.delete_raw", lambda r: {**sid(r), "startTime": r.args.hda_start, "endTime": r.args.hda_end, "serverHandles": []}),
        ProbeSpec("opcclassic.hda.delete_at_time", lambda r: {**sid(r), "serverHandles": [], "timestamps": []}),
        ProbeSpec("opcclassic.hda.insert_annotations", lambda r: {**sid(r), "serverHandles": [], "timestamps": [], "annotationTexts": [], "users": [], "annotationTimes": []}),
        ProbeSpec("opcclassic.hda.release_item_handles", lambda r: {**sid(r), "serverHandles": r.hda_item_handles}),
        ProbeSpec("opcclassic.hda.disconnect", sid),

        ProbeSpec("opcclassic.ae.connect", lambda r: r.dcom_args("ae")),
        ProbeSpec("opcclassic.ae.get_status", sid),
        ProbeSpec("opcclassic.ae.browse_areas", lambda r: {**sid(r), "areaQualifiedName": ""}),
        ProbeSpec("opcclassic.ae.query_event_categories", lambda r: {**sid(r), "eventTypes": "all"}, after_ae_categories),
        ProbeSpec("opcclassic.ae.query_event_attributes", lambda r: {**sid(r), "eventCategory": r.ae_category}, after_ae_attributes),
        ProbeSpec("opcclassic.ae.create_subscription", lambda r: {**sid(r), "active": True, "bufferTimeMs": 1000, "maxBufferSize": 0, "clientSubscription": 1}, after_subscription("ae_subscription_id")),
        ProbeSpec("opcclassic.ae.set_filter", lambda r: {**sid(r), "subscriptionId": r.ae_subscription_id, "eventTypes": "all", "eventCategories": ([r.ae_category] if r.ae_category else []), "minSeverity": 0, "maxSeverity": 1000, "areas": [], "sources": []}),
        ProbeSpec("opcclassic.ae.poll_events", lambda r: {**sid(r), "subscriptionId": r.ae_subscription_id, "maxNotifications": 10, "waitMilliseconds": 100}),
        ProbeSpec("opcclassic.ae.refresh_subscription", lambda r: {**sid(r), "subscriptionId": r.ae_subscription_id}),
        ProbeSpec("opcclassic.ae.get_condition_state", lambda r: {**sid(r), "source": r.args.ae_source, "conditionName": r.args.ae_condition, "attributeIds": r.ae_attribute_ids}),
        ProbeSpec("opcclassic.ae.ack_condition", lambda r: {**sid(r), "source": r.args.ae_source, "conditionName": r.args.ae_condition, "actor": "mcp-probe", "comment": "probe", "cookie": 0}),
        ProbeSpec("opcclassic.ae.cancel_subscription", lambda r: {**sid(r), "subscriptionId": r.ae_subscription_id}),
        ProbeSpec("opcclassic.ae.disconnect", sid),

        ProbeSpec("opcclassic.batch.connect", lambda r: r.dcom_args("batch")),
        ProbeSpec("opcclassic.batch.get_status", sid),
        ProbeSpec("opcclassic.batch.query_batch_summaries", lambda r: {**sid(r), "model": "OPCBBatchModel", "maxResults": 10}),
        ProbeSpec("opcclassic.batch.query_enumeration_sets", sid, after_batch_enumeration_sets),
        ProbeSpec("opcclassic.batch.query_enumeration", lambda r: {**sid(r), "enumerationSetId": r.batch_enum_set_id, "enumerationValue": 0}),
        ProbeSpec("opcclassic.batch.query_enumeration_list", lambda r: {**sid(r), "enumerationSetId": r.batch_enum_set_id}),
        ProbeSpec("opcclassic.batch.disconnect", sid),

        ProbeSpec("opcclassic.commands.connect", lambda r: r.dcom_args("commands")),
        ProbeSpec("opcclassic.commands.get_status", sid),
        ProbeSpec("opcclassic.commands.get_command_descriptions", lambda r: {**sid(r), "commandNamespace": "", "commandNames": []}),
        ProbeSpec("opcclassic.commands.invoke_command", lambda r: {**sid(r), "commandName": r.args.command_name, "commandNamespace": "", "targetId": "", "arguments": [], "filters": [], "asynchronous": True, "updateFrequencyMs": 1000, "keepAliveTimeMs": 30000}, after_command_invocation),
        ProbeSpec("opcclassic.commands.poll_command_state", lambda r: {**sid(r), "invocationId": r.command_invocation_id or r.args.command_invocation_id, "waitTimeMs": 0}),
        ProbeSpec("opcclassic.commands.cancel_command", lambda r: {**sid(r), "invocationId": r.command_invocation_id or r.args.command_invocation_id}),
        ProbeSpec("opcclassic.commands.disconnect", sid),

        ProbeSpec("opcclassic.dx.connect", lambda r: r.dcom_args("dx")),
        ProbeSpec("opcclassic.dx.get_status", sid),
        ProbeSpec("opcclassic.dx.query_connections", lambda r: {**sid(r), "browsePath": "", "connectionMasks": [], "recursive": False}),
        ProbeSpec("opcclassic.dx.query_source_servers", sid),
        ProbeSpec("opcclassic.dx.disconnect", sid),
        ProbeSpec("opcclassic.dx.add_source_server", lambda r: {**sid(r), "sourceServer": r.dx_source()}),
        ProbeSpec("opcclassic.dx.modify_source_server", lambda r: {**sid(r), "sourceServer": r.dx_source()}),
        ProbeSpec("opcclassic.dx.add_connection", lambda r: {**sid(r), "connection": r.dx_connection()}),
        ProbeSpec("opcclassic.dx.modify_connection", lambda r: {**sid(r), "connection": r.dx_connection()}),
        ProbeSpec("opcclassic.dx.update_connection", lambda r: {**sid(r), "connectionName": r.args.dx_connection_name, "connection": r.dx_connection(), "browsePath": "", "recursive": False}),
        ProbeSpec("opcclassic.dx.delete_connection", lambda r: {**sid(r), "connectionName": r.args.dx_connection_name, "browsePath": "", "recursive": False}),
        ProbeSpec("opcclassic.dx.reset_configuration", lambda r: {**sid(r), "configurationVersion": ""}),

        ProbeSpec("opcclassic.xmlda.connect", lambda r: {**sid(r), "endpointUrl": r.args.xmlda_endpoint}),
        ProbeSpec("opcclassic.xmlda.get_status", lambda r: {**sid(r), "clientRequestHandle": "probe-status"}),
        ProbeSpec("opcclassic.xmlda.browse", lambda r: {**sid(r), "itemName": "", "itemPath": "", "continuationPoint": "", "maxElementsReturned": 0, "browseFilter": "all", "elementNameFilter": "", "clientRequestHandle": "probe-browse"}),
        ProbeSpec("opcclassic.xmlda.get_properties", lambda r: {**sid(r), "itemNames": [r.args.da_read_item], "itemPath": "", "propertyNames": [], "returnAllProperties": True, "returnPropertyValues": False, "returnErrorText": True, "clientRequestHandle": "probe-properties"}),
        ProbeSpec("opcclassic.xmlda.read", lambda r: {**sid(r), "items": [{"itemName": r.args.da_read_item, "clientItemHandle": "read-1", "maxAge": 0}], "returnErrorText": True, "clientRequestHandle": "probe-read"}),
        ProbeSpec("opcclassic.xmlda.write", lambda r: {**sid(r), "items": [{"itemName": r.args.da_bucket_int_item, "value": r.args.da_write_int, "clientItemHandle": "write-1"}], "returnValuesOnReply": False, "returnErrorText": True, "clientRequestHandle": "probe-write"}),
        ProbeSpec("opcclassic.xmlda.subscribe", lambda r: {**sid(r), "items": [{"itemName": r.args.da_read_item, "clientItemHandle": "sub-1", "requestedSamplingRate": 1000, "deadband": 0}], "itemPath": "", "requestedSamplingRate": 1000, "subscriptionPingRate": 10000, "returnValuesOnReply": False, "returnErrorText": True, "enableBuffering": False, "clientRequestHandle": "probe-subscribe"}, after_xmlda_subscription),
        ProbeSpec("opcclassic.xmlda.poll_subscription", lambda r: {**sid(r), "serverSubHandles": ([r.xmlda_subscription_handle] if r.xmlda_subscription_handle else []), "waitTime": 0, "returnAllItems": True, "returnErrorText": True, "clientRequestHandle": "probe-poll"}),
        ProbeSpec("opcclassic.xmlda.cancel_subscription", lambda r: {**sid(r), "serverSubHandle": r.xmlda_subscription_handle or "probe", "clientRequestHandle": "probe-cancel"}),
        ProbeSpec("opcclassic.xmlda.disconnect", sid),

        ProbeSpec("opcclassic.session.close", sid),
    ]


def sid(runner: ProbeRunner) -> dict[str, Any]:
    return {"sessionId": runner.session_id}


def after_session_create(runner: ProbeRunner, value: Any) -> None:
    if isinstance(value, dict):
        runner.session_id = str(value.get("sessionId") or "")


def after_da_add_group(runner: ProbeRunner, value: Any) -> None:
    if isinstance(value, dict):
        runner.da_group_handle = int(value.get("serverGroupHandle") or 0)


def after_da_add_items(runner: ProbeRunner, value: Any) -> None:
    runner.da_item_handles = handles_from_results(value)


def after_hda_item_handles(runner: ProbeRunner, value: Any) -> None:
    runner.hda_item_handles = handles_from_results(value)


def after_hda_aggregates(runner: ProbeRunner, value: Any) -> None:
    if isinstance(value, list) and value:
        first = value[0]
        if isinstance(first, dict):
            runner.hda_aggregate = str(first.get("name") or first.get("aggregateId") or runner.hda_aggregate)


def after_ae_categories(runner: ProbeRunner, value: Any) -> None:
    if isinstance(value, list) and value:
        first = value[0]
        if isinstance(first, dict):
            runner.ae_category = int(first.get("eventCategory") or 0)


def after_ae_attributes(runner: ProbeRunner, value: Any) -> None:
    if isinstance(value, list):
        runner.ae_attribute_ids = [int(item.get("attributeId")) for item in value if isinstance(item, dict) and item.get("attributeId") is not None][:8]


def after_subscription(attr: str) -> ProbeAfter:
    def store(runner: ProbeRunner, value: Any) -> None:
        if isinstance(value, dict):
            setattr(runner, attr, str(value.get("subscriptionId") or ""))
    return store


def after_batch_enumeration_sets(runner: ProbeRunner, value: Any) -> None:
    if isinstance(value, list) and value:
        first = value[0]
        if isinstance(first, dict):
            runner.batch_enum_set_id = int(first.get("enumerationSetId") or first.get("id") or 0)


def after_command_invocation(runner: ProbeRunner, value: Any) -> None:
    if isinstance(value, dict):
        runner.command_invocation_id = str(value.get("invocationId") or "")


def after_xmlda_subscription(runner: ProbeRunner, value: Any) -> None:
    if isinstance(value, dict):
        runner.xmlda_subscription_handle = str(value.get("serverSubHandle") or "")


def handles_from_results(value: Any) -> list[int]:
    handles: list[int] = []
    if isinstance(value, list):
        for item in value:
            if not isinstance(item, dict):
                continue
            handle = item.get("serverHandle")
            succeeded = item.get("succeeded", True)
            if isinstance(handle, int) and handle != 0 and succeeded is not False:
                handles.append(handle)
    return handles


def make_result(tool: str, arguments: dict[str, Any], success: bool, error: Optional[str], summary: str) -> dict[str, Any]:
    return {
        "tool": tool,
        "args": redact(arguments),
        "success": success,
        "error": error,
        "summary": one_line(summary),
    }


def redact(value: Any) -> Any:
    if isinstance(value, dict):
        result: dict[str, Any] = {}
        for key, item in value.items():
            if any(secret in key.lower() for secret in ("password", "secret", "token")):
                result[key] = "***" if item is not None else None
            else:
                result[key] = redact(item)
        return result
    if isinstance(value, list):
        return [redact(item) for item in value]
    return value


def summarize(value: Any) -> str:
    if value is None:
        return "null"
    if isinstance(value, dict):
        if "message" in value:
            parts = [str(value.get("message"))]
            if "succeeded" in value:
                parts.append(f"succeeded={value.get('succeeded')}")
            if "hResult" in value:
                parts.append(f"hResult={format_hresult(value.get('hResult'))}")
            return " ".join(parts)
        if "sessionId" in value:
            return f"sessionId={value.get('sessionId')} daConnected={value.get('daConnected')}"
        if "vendorInfo" in value or "serverState" in value:
            return f"state={value.get('state') or value.get('serverState')} vendor={value.get('vendorInfo')} version={value.get('serverVersion') or value.get('productVersion')}"
        if "subscriptionId" in value:
            return f"subscriptionId={value.get('subscriptionId')} queued={value.get('queuedEventCount')} active={value.get('active')}"
        if "serverSubHandle" in value:
            items = value.get("items")
            count = len(items) if isinstance(items, list) else 0
            return f"serverSubHandle={value.get('serverSubHandle')} items={count} state={value.get('serverState')}"
        if "invocationId" in value:
            return f"invocationId={value.get('invocationId')} succeeded={value.get('succeeded')} message={value.get('message')}"
        if "elements" in value:
            elements = value.get("elements")
            return f"elements={len(elements) if isinstance(elements, list) else 0} state={value.get('serverState')} more={value.get('moreElements')}"
        if "itemLists" in value:
            lists = value.get("itemLists")
            return f"itemLists={len(lists) if isinstance(lists, list) else 0} overflow={value.get('dataBufferOverflow')}"
        keys = ",".join(list(value.keys())[:6])
        return f"object keys={keys}"
    if isinstance(value, list):
        if not value:
            return "count=0"
        first = value[0]
        prefix = f"count={len(value)}"
        if isinstance(first, dict):
            if "itemName" in first:
                return f"{prefix} firstItem={first.get('itemName')} value={first.get('value')} hr={format_hresult(first.get('hResult')) or first.get('resultCode')}"
            if "name" in first:
                return f"{prefix} firstName={first.get('name')}"
            if "qualifiedName" in first:
                return f"{prefix} firstArea={first.get('qualifiedName')}"
            if "eventCategory" in first:
                return f"{prefix} firstCategory={first.get('eventCategory')} {first.get('description')}"
            if "serverHandle" in first:
                return f"{prefix} firstHandle={first.get('serverHandle')} succeeded={first.get('succeeded')}"
            if "hResult" in first:
                return f"{prefix} firstHResult={format_hresult(first.get('hResult'))} message={first.get('message')}"
            return f"{prefix} firstKeys={','.join(list(first.keys())[:6])}"
        return f"{prefix} first={first!r}"
    return str(value)


def format_hresult(value: Any) -> str:
    if not isinstance(value, int):
        return ""
    return f"0x{value & 0xFFFFFFFF:08X}"


def one_line(text: str, limit: int = 240) -> str:
    collapsed = " ".join(str(text).split())
    return collapsed if len(collapsed) <= limit else collapsed[: limit - 1] + "…"


def first_line(text: str) -> str:
    line = str(text).strip().splitlines()[0] if str(text).strip() else ""
    return one_line(line, 400)


def default_for_schema(schema: dict[str, Any]) -> Any:
    typ = schema.get("type")
    if typ == "array":
        return []
    if typ == "integer":
        return 0
    if typ == "number":
        return 0
    if typ == "boolean":
        return False
    if typ == "object":
        return {}
    return ""


def parse_args() -> argparse.Namespace:
    parser = argparse.ArgumentParser(description="Probe Opc.Classic MCP tools and emit JSON results to stdout.")
    parser.add_argument("--host", default="localhost")
    parser.add_argument("--clsid", default=None, help="Default DCOM server CLSID used for DA/HDA/AE and optional DCOM specs.")
    parser.add_argument("--progid", default=None, help="Default DCOM server ProgID when a CLSID is not supplied.")
    parser.add_argument("--connection-string", default=None, help="Default MCP DCOM connection string override.")
    for kind in ("da", "hda", "ae", "batch", "commands", "dx"):
        parser.add_argument(f"--{kind}-clsid", default=None)
        parser.add_argument(f"--{kind}-progid", default=None)
        parser.add_argument(f"--{kind}-connection-string", default=None)
    parser.add_argument("--username", default=None)
    parser.add_argument("--password", default=None)
    parser.add_argument("--use-kerberos", action="store_true")
    parser.add_argument("--no-sso", action="store_true", help="Disable DA Windows SSO. SSO is also disabled when explicit credentials are supplied.")
    parser.add_argument("--request-timeout", type=float, default=60.0)
    parser.add_argument("--server-start-delay", type=float, default=2.0)

    parser.add_argument("--da-browse-branch", default="Random")
    parser.add_argument("--da-read-item", default="Random.Int4")
    parser.add_argument("--da-group-name", default="BB")
    parser.add_argument("--da-bucket-int-item", default="Bucket Brigade.Int4")
    parser.add_argument("--da-bucket-string-item", default="Bucket Brigade.String")
    parser.add_argument("--da-write-int", type=int, default=12345)
    parser.add_argument("--da-write-string", default="hello-from-mcp-probe")

    parser.add_argument("--hda-item", default="Random.Int4")
    parser.add_argument("--hda-start", default="NOW-1H")
    parser.add_argument("--hda-end", default="NOW")
    parser.add_argument("--hda-at-time", default="1970-01-01T00:00:00Z")

    parser.add_argument("--ae-source", default="Random.Int4")
    parser.add_argument("--ae-condition", default="Condition")
    parser.add_argument("--security-username", default="mcp_probe")
    parser.add_argument("--security-password", default="")
    parser.add_argument("--cpx-dictionary-id", default="OPCBinary")
    parser.add_argument("--command-name", default="__mcp_probe_noop__")
    parser.add_argument("--command-invocation-id", default="__mcp_probe_noop__")
    parser.add_argument("--dx-connection-name", default="__mcp_probe_connection__")
    parser.add_argument("--dx-source-name", default="__mcp_probe_source__")
    parser.add_argument("--xmlda-endpoint", default="inmemory://probe-missing")

    args = parser.parse_args()
    has_target = any(getattr(args, f"{kind}_clsid") or getattr(args, f"{kind}_progid") or getattr(args, f"{kind}_connection_string") for kind in ("da", "hda", "ae"))
    if not (args.clsid or args.progid or args.connection_string or has_target):
        parser.error("supply --clsid, --progid, --connection-string, or DA/HDA/AE-specific target options")
    args.use_sso = not args.no_sso and not (args.username or args.password)
    return args


def main() -> int:
    args = parse_args()
    repo_root = os.path.dirname(os.path.dirname(os.path.abspath(__file__)))
    proc: Optional[subprocess.Popen[bytes]] = None
    results: list[dict[str, Any]] = []
    try:
        proc = launch_server(repo_root)
        time.sleep(args.server_start_delay)
        client = McpClient(proc, args.request_timeout)
        client.initialize()
        tools = client.list_tools()
        runner = ProbeRunner(args, client)
        results = runner.run(tools)
        return 0
    except Exception as ex:
        results.append(make_result("__fatal__", {}, False, first_line(str(ex)), ""))
        return 1
    finally:
        if proc is not None:
            try:
                proc.terminate()
                proc.wait(timeout=5)
            except Exception:
                proc.kill()
        json.dump(results, sys.stdout, indent=2, default=str)
        sys.stdout.write("\n")


if __name__ == "__main__":
    sys.exit(main())
