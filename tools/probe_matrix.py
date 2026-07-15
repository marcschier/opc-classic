#!/usr/bin/env python3
"""Per-server expected-outcome matrices for the probe driver.

Tool names below are the live ones exposed by the MCP server in
mcp/Opc.Classic.Mcp/Tools/ as of this commit. Validate by running:

    grep 'McpServerTool\\(Name = "' mcp/Opc.Classic.Mcp/Tools/*.cs

A matrix maps each MCP tool name to one of:

- "PASS": the tool should succeed against this server profile
- "EXPECTED_FAIL": the tool is expected to fail with a documented reason
- "NOT_APPLICABLE": the tool targets a spec this server doesn't implement

When the probe driver is invoked with --expect-matrix <profile>, each result
row is annotated with its expected outcome and a classification verdict:

- "MATCH": the actual outcome matches the expected outcome
- "REGRESSION": expected PASS but actually failed (this is a real bug)
- "UNEXPECTED_PASS": expected EXPECTED_FAIL/NOT_APPLICABLE but actually passed
- "MISSING_CLASSIFICATION": tool was executed but the matrix has no entry

The driver exit code is 0 iff there are zero REGRESSION rows.
UNEXPECTED_PASS rows are informational (the matrix can usually be updated to
mark them as PASS).

This module is imported by tools/probe_servers.py.

Profile docs: interop/docs/probe-coverage.md.
"""

from __future__ import annotations

from typing import Any, Optional


# -- shared tool buckets -------------------------------------------------------


SESSION_TOOLS = {
    "opcclassic.session.create": "PASS",
    "opcclassic.session.list": "PASS",
    "opcclassic.session.close": "PASS",
}


CAPTURE_TOOLS = {
    "opcclassic.capture.list_interfaces": "PASS",
    "opcclassic.capture.start": "PASS",
    "opcclassic.capture.stop": "PASS",
    "opcclassic.capture.list": "PASS",
    "opcclassic.capture.get": "PASS",
    "opcclassic.capture.tail": "PASS",
    "opcclassic.capture.close_cursor": "PASS",
    "opcclassic.capture.set_filter": "PASS",
    "opcclassic.capture.subscribe_notifications": "PASS",
    "opcclassic.capture.unsubscribe_notifications": "PASS",
    "opcclassic.capture.summarize": "PASS",
    "opcclassic.capture.remove": "PASS",
    "opcclassic.capture.decode_pdu": "PASS",
    "opcclassic.capture.decode_file": "PASS",
    "opcclassic.capture.replay": "PASS",
    "opcclassic.capture.replay_file": "PASS",
}


SESSION_AND_CAPTURE = {
    **SESSION_TOOLS,
    **CAPTURE_TOOLS,
}


DA_TOOLS = (
    "opcclassic.da.connect",
    "opcclassic.da.disconnect",
    "opcclassic.da.get_status",
    "opcclassic.da.get_error_string",
    "opcclassic.da.browse",
    "opcclassic.da.get_properties",
    "opcclassic.da.add_group",
    "opcclassic.da.remove_group",
    "opcclassic.da.add_items",
    "opcclassic.da.read_sync",
    "opcclassic.da.write_sync",
    "opcclassic.da.subscribe",
    "opcclassic.da.poll_subscription",
)

HDA_TOOLS = (
    "opcclassic.hda.connect",
    "opcclassic.hda.disconnect",
    "opcclassic.hda.get_status",
    "opcclassic.hda.browse",
    "opcclassic.hda.get_aggregates",
    "opcclassic.hda.get_item_handles",
    "opcclassic.hda.release_item_handles",
    "opcclassic.hda.validate_items",
    "opcclassic.hda.read_raw",
    "opcclassic.hda.read_processed",
    "opcclassic.hda.read_at_time",
    "opcclassic.hda.read_modified",
    "opcclassic.hda.read_attribute",
    "opcclassic.hda.read_annotations",
    "opcclassic.hda.insert_data",
    "opcclassic.hda.insert_replace_data",
    "opcclassic.hda.insert_annotations",
    "opcclassic.hda.replace_data",
    "opcclassic.hda.delete_raw",
    "opcclassic.hda.delete_at_time",
)

AE_TOOLS = (
    "opcclassic.ae.connect",
    "opcclassic.ae.disconnect",
    "opcclassic.ae.get_status",
    "opcclassic.ae.browse_areas",
    "opcclassic.ae.query_event_categories",
    "opcclassic.ae.query_event_attributes",
    "opcclassic.ae.create_subscription",
    "opcclassic.ae.refresh_subscription",
    "opcclassic.ae.poll_events",
    "opcclassic.ae.cancel_subscription",
    "opcclassic.ae.set_filter",
    "opcclassic.ae.ack_condition",
    "opcclassic.ae.get_condition_state",
)

BATCH_TOOLS = (
    "opcclassic.batch.connect",
    "opcclassic.batch.disconnect",
    "opcclassic.batch.get_status",
    "opcclassic.batch.query_batch_summaries",
    "opcclassic.batch.query_enumeration",
    "opcclassic.batch.query_enumeration_list",
    "opcclassic.batch.query_enumeration_sets",
)

COMMANDS_TOOLS = (
    "opcclassic.commands.connect",
    "opcclassic.commands.disconnect",
    "opcclassic.commands.get_status",
    "opcclassic.commands.cancel_command",
    "opcclassic.commands.get_command_descriptions",
    "opcclassic.commands.invoke_command",
    "opcclassic.commands.poll_command_state",
)

DX_TOOLS = (
    "opcclassic.dx.connect",
    "opcclassic.dx.disconnect",
    "opcclassic.dx.get_status",
    "opcclassic.dx.add_connection",
    "opcclassic.dx.add_source_server",
    "opcclassic.dx.delete_connection",
    "opcclassic.dx.modify_connection",
    "opcclassic.dx.modify_source_server",
    "opcclassic.dx.query_connections",
    "opcclassic.dx.query_source_servers",
    "opcclassic.dx.reset_configuration",
    "opcclassic.dx.update_connection",
)

XMLDA_TOOLS = (
    "opcclassic.xmlda.connect",
    "opcclassic.xmlda.disconnect",
    "opcclassic.xmlda.get_status",
    "opcclassic.xmlda.browse",
    "opcclassic.xmlda.get_properties",
    "opcclassic.xmlda.read",
    "opcclassic.xmlda.write",
    "opcclassic.xmlda.subscribe",
    "opcclassic.xmlda.poll_subscription",
    "opcclassic.xmlda.cancel_subscription",
)

CPX_TOOLS = (
    "opcclassic.cpx.get_complex_type",
    "opcclassic.cpx.get_dictionary",
    "opcclassic.cpx.get_type_system",
)

SECURITY_TOOLS = (
    "opcclassic.security.is_available_nt",
    "opcclassic.security.is_available_private",
    "opcclassic.security.logon",
    "opcclassic.security.logoff",
)

DISCOVERY_TOOLS = ("opcclassic.discovery.enumerate_servers",)


def _all(names: tuple[str, ...], outcome: str) -> dict[str, str]:
    return {name: outcome for name in names}


# Disconnect tools are marked Idempotent=true in the MCP server and no-op
# gracefully when no connection exists for the session. They always succeed
# regardless of whether the server profile implements that spec, so they
# never belong in the NOT_APPLICABLE bucket -- they always classify as PASS.
DISCONNECT_TOOLS_ALWAYS_PASS = {
    "opcclassic.da.disconnect": "PASS",
    "opcclassic.hda.disconnect": "PASS",
    "opcclassic.ae.disconnect": "PASS",
    "opcclassic.batch.disconnect": "PASS",
    "opcclassic.commands.disconnect": "PASS",
    "opcclassic.dx.disconnect": "PASS",
    "opcclassic.xmlda.disconnect": "PASS",
}


# -- per-profile matrices ------------------------------------------------------


def _da_205a_matrix() -> dict[str, str]:
    """OPC Foundation OpcTestServer_x64.exe + OpcSecurityServer.

    Pure DA 2.05a. IOPCItemIO (opcclassic.da.read_items_by_id) is DA 3.0 and
    rejected with E_NOINTERFACE.
    """
    matrix: dict[str, str] = {}
    matrix.update(SESSION_AND_CAPTURE)
    matrix.update(_all(DISCOVERY_TOOLS, "PASS"))
    matrix.update(_all(DA_TOOLS, "PASS"))
    # IOPCItemIO is DA 3.0 only.
    matrix["opcclassic.da.read_items_by_id"] = "EXPECTED_FAIL"
    # CPX: not implemented by stock DA servers without OPCBinary.
    matrix.update(_all(CPX_TOOLS, "EXPECTED_FAIL"))
    # Security: only implemented by OpcSecurityServer profile.
    matrix.update(_all(SECURITY_TOOLS, "EXPECTED_FAIL"))
    # Wrong-spec tools.
    matrix.update(_all(HDA_TOOLS, "NOT_APPLICABLE"))
    matrix.update(_all(AE_TOOLS, "NOT_APPLICABLE"))
    matrix.update(_all(BATCH_TOOLS, "NOT_APPLICABLE"))
    matrix.update(_all(COMMANDS_TOOLS, "NOT_APPLICABLE"))
    matrix.update(_all(DX_TOOLS, "NOT_APPLICABLE"))
    matrix.update(_all(XMLDA_TOOLS, "NOT_APPLICABLE"))
    return matrix


def _testserver_matrix() -> dict[str, str]:
    """OPC Foundation OpcTestServer_x64.exe specifically.

    TestServer's OpcTestServer.cpp advertises BOTH CATID_OPCDAServer20 AND
    CATID_OPCDAServer30 (a divergence vs. upstream) so it implements
    DA 3.0 IOPCItemIO. The COpcTestServer also implements IOPCTypeSystem so
    cpx.get_type_system can negotiate (returns empty supported list).
    """
    matrix = _da_205a_matrix()
    matrix["opcclassic.da.read_items_by_id"] = "PASS"
    matrix["opcclassic.cpx.get_type_system"] = "PASS"
    return matrix


def _da_3_matrix() -> dict[str, str]:
    """Matrikon + our DaServer + CttServer (DA 2.05a + DA 3.0).

    Matrikon implements IOPCTypeSystem returning a (typically empty)
    supported list, so cpx.get_type_system is PASS. The other CPX tools
    still EXPECTED_FAIL unless an OPCBinary dictionary is registered.

    Security tools always return success (true/false based on server reply
    or false-on-OpcException for servers that don't implement them), so
    they pass against any DA-capable server.
    """
    matrix = _da_205a_matrix()
    matrix["opcclassic.da.read_items_by_id"] = "PASS"
    matrix["opcclassic.cpx.get_type_system"] = "PASS"
    for name in SECURITY_TOOLS:
        matrix[name] = "PASS"
    return matrix


def _hda_matrix() -> dict[str, str]:
    matrix: dict[str, str] = {}
    matrix.update(SESSION_AND_CAPTURE)
    matrix.update(_all(DISCOVERY_TOOLS, "PASS"))
    matrix.update(_all(HDA_TOOLS, "PASS"))
    # Wrong spec.
    matrix.update(_all(DA_TOOLS + ("opcclassic.da.read_items_by_id",), "NOT_APPLICABLE"))
    matrix.update(_all(AE_TOOLS, "NOT_APPLICABLE"))
    matrix.update(_all(BATCH_TOOLS, "NOT_APPLICABLE"))
    matrix.update(_all(COMMANDS_TOOLS, "NOT_APPLICABLE"))
    matrix.update(_all(DX_TOOLS, "NOT_APPLICABLE"))
    matrix.update(_all(XMLDA_TOOLS, "NOT_APPLICABLE"))
    matrix.update(_all(CPX_TOOLS, "NOT_APPLICABLE"))
    matrix.update(_all(SECURITY_TOOLS, "NOT_APPLICABLE"))
    return matrix


def _ae_matrix() -> dict[str, str]:
    matrix: dict[str, str] = {}
    matrix.update(SESSION_AND_CAPTURE)
    matrix.update(_all(DISCOVERY_TOOLS, "PASS"))
    matrix.update(_all(AE_TOOLS, "PASS"))
    # PERMANENT WAIVER (DR32/DR33, conclusively verified 2026-06-10).
    # opcae_ps.dll (the OPC Foundation native MIDL proxy/stub) crashes on the
    # response/request mid-call for these 2 methods. Confirmed via the
    # following multi-step investigation:
    #
    # Wire-format spec: extracted the authoritative wire format from
    # the vendored interop/inc/opc_ae_p.c MIDL-generated proxy/stub source.
    # Spec doc at docs/conformance/ae-wire-format.md. Identified that
    # szSource/szConditionName (GetConditionState) and szAcknowledgerID/
    # szComment (AckCondition) are marked [simple ref] (flags 0x10b, FC_RP
    # [simple_pointer] FC_C_WSTRING) -- the body must follow the FC_C_WSTRING
    # convention directly, with no outer 4-byte [unique] referent ID.
    #
    # Wire-byte capture: captured the managed encoder's actual wire
    # bytes via tests/Opc.Classic.Ae.Tests/Wire/Dr3233/Dr3233WireCaptureTests.cs.
    # Confirmed the encoder was emitting outer 4-byte referent IDs (visible
    # at offsets 0/0x28 of GetConditionState and 0x04/0x24 of AckCondition).
    #
    # Encoder fix: applied [OpcRefString] to all 4 simple_ref
    # scalar LPWSTR params in src/Opc.Classic.Ae/Dcom/IOPCInterfaces.cs.
    # Regenerated fixtures verify the wire bytes now match the MIDL
    # spec byte-for-byte: GetConditionState request 100->92 bytes (-8 bytes
    # = 2 referent IDs removed); AckCondition request 296->288 bytes (-8 bytes
    # = 2 referent IDs removed). All regression tests green.
    #
    # Elevated matrix re-run (transcript at
    # matrix-out/dr3233-phase-e-transcript.log): with spec-compliant
    # wire bytes, opcae_ps.dll STILL forcibly closes the connection. Client
    # observes "SocketException (10054): An existing connection was forcibly
    # closed by the remote host" on the response read for get_condition_state
    # and on the request write for ack_condition. The residual failure is
    # therefore confirmed to be in the vendor proxy/stub itself, not in the
    # managed encoder.
    #
    # PERMANENT DISPOSITION: keep these 2 markers as EXPECTED_FAIL on the
    # native-CCW samples-ae profile. The samples-ae-managed profile
    # bypasses opcae_ps.dll entirely via tcp:// direct
    # connect and flips these tools to PASS via _ae_managed_matrix() --
    # that is the recommended operational path for consumers needing AE
    # condition-state methods. See docs/CONFORMANCE.md "Documented waiver"
    # section for the full chain of evidence.
    matrix["opcclassic.ae.get_condition_state"] = "EXPECTED_FAIL"
    matrix["opcclassic.ae.ack_condition"] = "EXPECTED_FAIL"
    # Wrong spec.
    matrix.update(_all(DA_TOOLS + ("opcclassic.da.read_items_by_id",), "NOT_APPLICABLE"))
    matrix.update(_all(HDA_TOOLS, "NOT_APPLICABLE"))
    matrix.update(_all(BATCH_TOOLS, "NOT_APPLICABLE"))
    matrix.update(_all(COMMANDS_TOOLS, "NOT_APPLICABLE"))
    matrix.update(_all(DX_TOOLS, "NOT_APPLICABLE"))
    matrix.update(_all(XMLDA_TOOLS, "NOT_APPLICABLE"))
    matrix.update(_all(CPX_TOOLS, "NOT_APPLICABLE"))
    matrix.update(_all(SECURITY_TOOLS, "NOT_APPLICABLE"))
    return matrix


def _ae_managed_matrix() -> dict[str, str]:
    """AE matrix for the `samples-ae-managed` profile.

    Identical to `_ae_matrix()` except that the two AE tools waived on the
    native-CCW + opcae_ps.dll path become real PASS expectations on the
    managed-listener path. The managed TCP listener routes calls through
    the source-generated dispatchers (no native MIDL stub on the wire),
    so `GetConditionState` / `AckCondition` round-trip cleanly through the
    managed AE dispatcher implemented by SampleAeServer. The managed-
    listener path is connected by feeding `--ae-connection-string
    tcp://host:port` to the probe driver, which lands at the
    `tcp://`-scheme branch in `DefaultOpcAeConnectionFactory.ConnectAsync`.
    The native-CCW profile (`samples-ae`) keeps the EXPECTED_FAIL waiver
    as the conformance reference per `docs/CONFORMANCE.md` AE section.
    """
    matrix = _ae_matrix()
    matrix["opcclassic.ae.get_condition_state"] = "PASS"
    matrix["opcclassic.ae.ack_condition"] = "PASS"
    return matrix


def _security_da_matrix() -> dict[str, str]:
    """OpcSecurityServer = DA 2.05a + DA 3.0 IOPCItemIO + IOPCSecurityNT + IOPCSecurityPrivate.

    The sample security server now uses the unified OpcDaServerCcw which
    exposes IOPCBrowse / IOPCItemProperties / IOPCItemIO tearoffs (DA 3.0)
    and IOPCSecurityNT / IOPCSecurityPrivate tearoffs (OPC Security 1.00).
    """
    matrix = _da_3_matrix()
    for name in SECURITY_TOOLS:
        matrix[name] = "PASS"
    return matrix


PROFILES: dict[str, dict[str, str]] = {
    "testserver": _testserver_matrix(),
    "matrikon": _da_3_matrix(),
    "samples-da": _da_3_matrix(),
    "ctt-da": _da_3_matrix(),
    "samples-hda": _hda_matrix(),
    "samples-ae": _ae_matrix(),
    "samples-ae-managed": _ae_managed_matrix(),
    "security-da": _security_da_matrix(),
}

# Disconnect tools are idempotent + no-op-when-not-connected, so they
# unconditionally PASS regardless of profile. Apply the override AFTER the
# per-profile NOT_APPLICABLE wholesale assignments.
for _profile in PROFILES.values():
    _profile.update(DISCONNECT_TOOLS_ALWAYS_PASS)


def classify(profile_name: str, tool: str, success: bool, npcap_available: bool = True) -> tuple[str, str]:
    """Return (expected_outcome, verdict) for a result row.

    expected_outcome is one of: PASS / EXPECTED_FAIL / NOT_APPLICABLE /
    MISSING_CLASSIFICATION.

    verdict is one of: MATCH / REGRESSION / UNEXPECTED_PASS /
    MISSING_CLASSIFICATION.
    """
    matrix = PROFILES.get(profile_name)
    if matrix is None:
        return ("MISSING_CLASSIFICATION", "MISSING_CLASSIFICATION")

    expected = matrix.get(tool, "MISSING_CLASSIFICATION")
    if expected != "MISSING_CLASSIFICATION" and not npcap_available and tool in CAPTURE_TOOLS:
        expected = "EXPECTED_FAIL"
    if expected == "MISSING_CLASSIFICATION":
        return (expected, "MISSING_CLASSIFICATION")

    if expected == "PASS":
        return (expected, "MATCH" if success else "REGRESSION")

    # EXPECTED_FAIL and NOT_APPLICABLE both mean: should not succeed.
    if success:
        return (expected, "UNEXPECTED_PASS")
    return (expected, "MATCH")


def summarize_verdicts(results: list[dict[str, Any]]) -> dict[str, int]:
    summary: dict[str, int] = {}
    for row in results:
        verdict = row.get("verdict")
        if not isinstance(verdict, str):
            continue
        summary[verdict] = summary.get(verdict, 0) + 1
    return summary


def has_regressions(results: list[dict[str, Any]]) -> bool:
    return summarize_verdicts(results).get("REGRESSION", 0) > 0


def known_profile_names() -> list[str]:
    return sorted(PROFILES.keys())


def annotate(
    results: list[dict[str, Any]],
    profile_name: Optional[str],
    npcap_available: bool = True,
) -> list[dict[str, Any]]:
    """Add expectedOutcome + verdict columns to each row. No-op when
    profile_name is None."""
    if not profile_name:
        return results
    for row in results:
        tool = row.get("tool")
        success = bool(row.get("success"))
        if isinstance(tool, str):
            expected, verdict = classify(profile_name, tool, success, npcap_available=npcap_available)
            row["expectedOutcome"] = expected
            row["verdict"] = verdict
    return results
