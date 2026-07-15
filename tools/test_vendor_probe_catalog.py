# Copyright (c) 2026 Opc.Classic Contributors. Licensed under the MIT License.

from __future__ import annotations

import argparse
import json
import math
import os
import shutil
import sys
import unittest
from pathlib import Path
from types import SimpleNamespace
from unittest.mock import patch


HERE = Path(__file__).resolve().parent
REPO = HERE.parent
if str(HERE) not in sys.path:
    sys.path.insert(0, str(HERE))

import probe_servers
import run_cross_impl_matrix
import vendor_probe_catalog as vendor


class FakeClient:
    def __init__(self) -> None:
        self.calls: list[tuple[str, dict[str, object]]] = []

    def call_tool(self, name: str, arguments: dict[str, object]) -> object:
        self.calls.append((name, arguments))
        if name == "opcclassic.session.create":
            return {"sessionId": "session-1"}
        if name == "opcclassic.da.write_sync":
            return [{"itemId": "Vendor.Writable", "hResult": 0}]
        if name == "opcclassic.da.browse":
            return [{"itemId": "Vendor.Branch"}]
        return {"status": "ok"}


def probe_args(**overrides: object) -> SimpleNamespace:
    values: dict[str, object] = {
        "probe": None,
        "probe_scenarios": [],
        "probe_workflows": [],
        "host": "localhost",
        "progid": None,
        "clsid": None,
        "connection_string": None,
        "da_progid": "Vendor.Server.1",
        "da_clsid": None,
        "da_connection_string": None,
        "username": None,
        "password": None,
        "use_kerberos": False,
        "auth_level": None,
        "use_sso": True,
        "da_browse_branch": "",
        "da_browse_filter": "all",
        "da_group_name": "VendorProbe",
        "da_group_active": True,
        "da_update_rate_ms": 500,
        "da_item_ids": ["Vendor.Writable"],
        "da_client_handles": [1],
        "da_write_values": [1],
        "da_read_from_cache": False,
        "da_subscription_from_cache": False,
        "da_max_notifications": 1,
    }
    values.update(overrides)
    return SimpleNamespace(**values)


def matrix_args(**overrides: object) -> argparse.Namespace:
    values: dict[str, object] = {
        "use_clsid": False,
        "host": "localhost",
        "request_timeout": 60.0,
        "username": None,
        "password": None,
        "use_kerberos": False,
        "auth_level": None,
        "save_wire_payloads": None,
        "include_sensitive_results": False,
    }
    values.update(overrides)
    return argparse.Namespace(**values)


class VendorProbeCatalogTests(unittest.TestCase):
    def setUp(self) -> None:
        self.scratch = REPO / "artifacts" / f"vendor-probe-{os.getpid()}"
        self.scratch.mkdir(parents=True, exist_ok=True)

    def tearDown(self) -> None:
        shutil.rmtree(self.scratch, ignore_errors=True)

    def test_catalog_selection_and_execution_plan(self) -> None:
        _, descriptor = vendor.load_descriptor("generic-opc-classic-template")
        selected = vendor.selected_catalog_probes(descriptor)
        plan = vendor.descriptor_execution_plan(descriptor)

        self.assertEqual(len(plan), len(selected))
        self.assertEqual(
            {item["probeId"] for item in plan},
            {probe["id"] for probe in selected},
        )
        self.assertEqual(
            next(item for item in plan if item["probeId"] == "da-reconnect")["execution"],
            "workflow",
        )
        self.assertEqual(
            next(item for item in plan if item["probeId"] == "da-failover")["execution"],
            "workflow",
        )
        self.assertIn(
            "opcclassic.ae.refresh_subscription",
            vendor.selected_probe_tools(descriptor),
        )
        reduced = json.loads(json.dumps(descriptor))
        reduced["capabilities"].remove("ae-condition-state")
        self.assertNotIn(
            "ae-condition-state",
            {probe["id"] for probe in vendor.selected_catalog_probes(reduced)},
        )

    def test_final_arguments_reach_parser(self) -> None:
        _, descriptor = vendor.load_descriptor("generic-opc-classic-template")
        argv = vendor.final_probe_arguments(descriptor)
        with patch.object(
            sys,
            "argv",
            ["probe_servers.py", "--da-progid", descriptor["target"]["progid"], *argv],
        ):
            arguments = probe_servers.parse_args()

        self.assertEqual(arguments.da_read_item, "Vendor.Writable")
        self.assertEqual(arguments.da_item_ids, ["Vendor.Writable"])
        self.assertEqual(arguments.da_write_values, [1])
        self.assertEqual(arguments.ae_source, "Vendor.Source")
        self.assertEqual(arguments.hda_item, "Vendor.History")
        self.assertEqual(arguments.hda_start, "NOW-1H")

    def test_verdicts_block_declared_external_failures(self) -> None:
        _, descriptor = vendor.load_descriptor("matrikon")
        probe = next(value for value in descriptor["probes"] if value["id"] == "da-reconnect")

        self.assertEqual(
            vendor.classify_probe(probe, False, "DCOM E_ACCESSDENIED"),
            "BLOCKED",
        )
        self.assertEqual(
            vendor.classify_probe(probe, False, "decode failed"),
            "REGRESSION",
        )
        self.assertEqual(vendor.classify_probe(probe, True), "MATCH")

    def test_fixture_decoders_validate_protocol_shapes(self) -> None:
        _, descriptor = vendor.load_descriptor("generic-opc-classic-template")

        malformed = vendor.decode_fixture(descriptor, "da-malformed")
        truncated = vendor.decode_fixture(descriptor, "da-truncated")
        da_extension = vendor.decode_fixture(descriptor, "da-vendor-extension")
        ae_standard = vendor.decode_fixture(descriptor, "ae-condition-state-standard")
        ae_empty = vendor.decode_fixture(descriptor, "ae-condition-state-empty")
        ae_extension = vendor.decode_fixture(
            descriptor,
            "ae-condition-state-vendor-extension",
        )
        hda_standard = vendor.decode_fixture(descriptor, "hda-relative-time-standard")
        hda_extension = vendor.decode_fixture(
            descriptor,
            "hda-relative-time-vendor-extension",
        )

        self.assertEqual(malformed["verdict"], "MATCH")
        self.assertEqual(malformed["actual"]["errorCode"], "FIXTURE_HEX_INVALID")
        self.assertEqual(truncated["actual"]["errorCode"], "FIXTURE_HEX_TRUNCATED")
        self.assertEqual(
            da_extension["actual"]["decoded"]["decoder"],
            "opc-da-vendor-extension",
        )
        self.assertEqual(da_extension["actual"]["decoded"]["revision"], 1)
        self.assertEqual(ae_standard["actual"]["decoded"]["state"], 1)
        self.assertEqual(ae_standard["actual"]["decoded"]["quality"], 2)
        self.assertTrue(ae_empty["actual"]["decoded"]["empty"])
        self.assertEqual(ae_extension["actual"]["decoded"]["payloadLength"], 4)
        self.assertEqual(hda_standard["actual"]["decoded"]["expression"], "NOW")
        self.assertEqual(hda_extension["actual"]["decoded"]["expression"], "NOW-1D")

    def test_schema_validation_fails_closed_for_nested_shape_and_tokens(self) -> None:
        _, descriptor = vendor.load_descriptor("generic-opc-classic-template")

        cases: list[tuple[dict[str, object], str]] = []
        extra = json.loads(json.dumps(descriptor))
        extra["arguments"]["da"]["unexpected"] = True
        cases.append((extra, "additional property"))
        missing = json.loads(json.dumps(descriptor))
        del missing["arguments"]["da"]["groupName"]
        cases.append((missing, "required property"))
        unsafe_token = json.loads(json.dumps(descriptor))
        unsafe_token["prerequisites"][0]["artifact"]["rootToken"] = "bad-token"
        cases.append((unsafe_token, "safe pattern"))
        traversal = json.loads(json.dumps(descriptor))
        traversal["fixtures"][0]["path"] = "fixtures/../escape.hex"
        cases.append((traversal, "safe pattern"))

        for value, message in cases:
            with self.subTest(message=message):
                with self.assertRaisesRegex(vendor.VendorDescriptorError, message):
                    vendor.validate_descriptor(value)

        non_finite = json.loads(json.dumps(descriptor))
        non_finite["arguments"]["hda"]["resampleIntervalSeconds"] = math.inf
        with self.assertRaisesRegex(vendor.VendorDescriptorError, "non-finite"):
            vendor.validate_descriptor(non_finite)
        with self.assertRaisesRegex(vendor.VendorDescriptorError, "safe kebab-case"):
            vendor.load_descriptor("../outside")

    def test_resolved_paths_remain_inside_allowlisted_roots(self) -> None:
        with self.assertRaisesRegex(vendor.VendorDescriptorError, "safe relative"):
            vendor.resolve_contained_path(self.scratch.resolve(), "../outside.exe")

        _, descriptor = vendor.load_descriptor("testserver")
        unresolved = vendor.external_prerequisite_results(descriptor, {})
        relative = vendor.external_prerequisite_results(
            descriptor,
            {"OPC_TESTSERVER_INSTALL_ROOT": "relative"},
        )
        escaped_descriptor = json.loads(json.dumps(descriptor))
        escaped_descriptor["prerequisites"][0]["artifact"]["relativePath"] = "../outside.exe"
        escaped = vendor.external_prerequisite_results(
            escaped_descriptor,
            {"OPC_TESTSERVER_INSTALL_ROOT": str(self.scratch.resolve())},
        )

        self.assertEqual(
            unresolved[0]["actual"]["code"],
            "INSTALL_ROOT_NOT_PROVIDED",
        )
        self.assertEqual(relative[0]["actual"]["code"], "INSTALL_ROOT_INVALID")
        self.assertEqual(escaped[0]["actual"]["code"], "PATH_ESCAPE")
        self.assertEqual(escaped[0]["verdict"], "REGRESSION")

    def test_every_selected_probe_has_exactly_one_result(self) -> None:
        _, descriptor = vendor.load_descriptor("generic-opc-classic-template")
        results = run_cross_impl_matrix.finalize_descriptor_results(descriptor, [])
        selected_ids = [
            probe["id"] for probe in vendor.selected_catalog_probes(descriptor)
        ]

        self.assertEqual(len(results), len(selected_ids))
        self.assertEqual([row["probeId"] for row in results], selected_ids)
        self.assertEqual(len({row["probeId"] for row in results}), len(selected_ids))
        missing_mapping = next(
            row for row in results if row["probeId"] == "da-optional-interface-query"
        )
        self.assertEqual(missing_mapping["verdict"], "REGRESSION")
        self.assertEqual(
            missing_mapping["actual"]["errorCode"],
            "PROBE_MAPPING_MISSING",
        )

    def test_duplicate_descriptor_rows_collapse_to_one_regression(self) -> None:
        _, descriptor = vendor.load_descriptor("testserver")
        duplicate = {
            "probeId": "da-sync-write",
            "tool": "opcclassic.da.write_sync",
            "success": True,
            "result": [{"itemId": "Test.Int32", "hResult": 0}],
        }
        results = run_cross_impl_matrix.finalize_descriptor_results(
            descriptor,
            [duplicate, dict(duplicate)],
        )
        row = next(value for value in results if value["probeId"] == "da-sync-write")

        self.assertEqual(row["verdict"], "REGRESSION")
        self.assertEqual(
            row["actual"]["errorCode"],
            "DUPLICATE_PROBE_RESULTS",
        )

    def test_unavailable_tool_produces_descriptor_result(self) -> None:
        client = FakeClient()
        runner = probe_servers.ProbeRunner(
            probe_args(
                probe_scenarios=[
                    ("missing-tool", "opcclassic.da.get_status"),
                ],
            ),
            client,
        )
        results = runner.run([
            {"name": "opcclassic.session.create", "inputSchema": {}},
        ])
        row = next(value for value in results if value.get("probeId") == "missing-tool")

        self.assertFalse(row["success"])
        self.assertIn("TOOL_UNAVAILABLE", row["error"])

    def test_multiple_same_tool_scenarios_are_preserved(self) -> None:
        descriptor = {
            "capabilities": ["sync-write"],
            "probes": [
                {
                    "id": "one",
                    "type": "da-sync-write",
                    "requires": ["sync-write"],
                    "tool": "opcclassic.da.write_sync",
                },
                {
                    "id": "two",
                    "type": "da-sync-write",
                    "requires": ["sync-write"],
                    "tool": "opcclassic.da.write_sync",
                },
            ],
        }
        self.assertEqual(
            vendor.selected_probe_scenarios(descriptor),
            [
                {"probeId": "one", "tool": "opcclassic.da.write_sync"},
                {"probeId": "two", "tool": "opcclassic.da.write_sync"},
            ],
        )
        client = FakeClient()
        runner = probe_servers.ProbeRunner(
            probe_args(
                probe_scenarios=[
                    ("one", "opcclassic.da.write_sync"),
                    ("two", "opcclassic.da.write_sync"),
                ],
            ),
            client,
        )
        rows = runner.run([
            {"name": "opcclassic.da.write_sync", "inputSchema": {}},
        ])

        self.assertEqual(
            [row["probeId"] for row in rows if "probeId" in row],
            ["one", "two"],
        )
        self.assertEqual(
            len([call for call in client.calls if call[0] == "opcclassic.da.write_sync"]),
            2,
        )

    def test_reconnect_and_failover_run_disconnect_reconnect_followup(self) -> None:
        client = FakeClient()
        runner = probe_servers.ProbeRunner(
            probe_args(
                probe_workflows=[
                    ("reconnect", "da-reconnect"),
                    ("failover", "da-failover"),
                ],
            ),
            client,
        )
        tools = [
            {"name": name, "inputSchema": {}}
            for name in (
                "opcclassic.session.create",
                "opcclassic.da.connect",
                "opcclassic.da.disconnect",
                "opcclassic.da.get_status",
                "opcclassic.da.browse",
            )
        ]
        rows = runner.run(tools)

        reconnect = next(row for row in rows if row.get("probeId") == "reconnect")
        failover = next(row for row in rows if row.get("probeId") == "failover")
        self.assertTrue(reconnect["success"])
        self.assertTrue(failover["success"])
        self.assertEqual(len(reconnect["result"]["steps"]), 5)
        self.assertEqual(len(failover["result"]["steps"]), 5)
        self.assertEqual(reconnect["result"]["steps"][-1]["tool"], "opcclassic.da.get_status")
        self.assertEqual(failover["result"]["steps"][-1]["tool"], "opcclassic.da.browse")
        self.assertGreaterEqual(
            len([call for call in client.calls if call[0] == "opcclassic.da.disconnect"]),
            4,
        )

    def test_expected_item_is_matched_not_first_result(self) -> None:
        probe = {
            "expected": {
                "outcome": "success",
                "itemId": "Second.Item",
                "hResult": "0x00000000",
            },
            "expectedFailures": [],
        }
        verdict, actual = vendor.evaluate_probe_result(
            probe,
            {
                "success": True,
                "result": [
                    {"itemId": "First.Item", "hResult": 0x80004005},
                    {"itemId": "Second.Item", "hResult": 0},
                ],
            },
        )

        self.assertEqual(verdict, "MATCH")
        self.assertTrue(actual["itemMatched"])
        self.assertEqual(actual["hResult"], "0x00000000")

    def test_loader_and_final_args_reject_nonfinite(self) -> None:
        for constant in ("NaN", "Infinity", "-Infinity"):
            with self.assertRaisesRegex(vendor.VendorDescriptorError, "Non-finite"):
                vendor.load_descriptor_json('{"value":' + constant + "}")
        _, descriptor = vendor.load_descriptor("generic-opc-classic-template")
        descriptor = json.loads(json.dumps(descriptor))
        descriptor["arguments"]["da"]["writeValues"][0] = math.nan
        with self.assertRaisesRegex(vendor.VendorDescriptorError, "non-finite"):
            vendor.final_probe_arguments(descriptor)
        with self.assertRaisesRegex(ValueError, "non-finite"):
            probe_servers.require_finite_numbers({"value": math.inf}, "$")

    def test_reports_are_allowlisted_unless_sensitive_flag_is_explicit(self) -> None:
        row = {
            "probeId": "privacy",
            "tool": "opcclassic.da.read_sync",
            "args": {"itemIds": ["Plant.Secret"], "password": "***"},
            "success": True,
            "result": [{"itemId": "Plant.Secret", "value": 42}],
            "summary": "Plant.Secret=42",
            "error": r"C:\private\server.exe failed",
            "expectedOutcome": "PASS",
            "verdict": "MATCH",
            "actual": {
                "outcome": "success",
                "decoded": {
                    "decoder": "opc-hda-relative-time",
                    "expression": "NOW-1D",
                    "unit": "D",
                    "magnitude": 1,
                },
            },
        }

        safe = run_cross_impl_matrix.persisted_results([row], False)[0]
        sensitive = run_cross_impl_matrix.persisted_results([row], True)[0]

        self.assertNotIn("args", safe)
        self.assertNotIn("result", safe)
        self.assertNotIn("summary", safe)
        self.assertNotIn("error", safe)
        self.assertNotIn("expression", safe["actual"]["decoded"])
        self.assertIn("args", sensitive)
        self.assertIn("result", sensitive)
        aggregate = run_cross_impl_matrix.persisted_matrix_entry(
            {
                "profile": "testserver",
                "skip_reason": r"stale C:\private\server.exe",
                "skip_code": "SERVER_NOT_REGISTERED",
            },
            False,
        )
        self.assertNotIn("skip_reason", aggregate)
        self.assertEqual(aggregate["skip_code"], "SERVER_NOT_REGISTERED")

    def test_aggregate_regressions_are_recursively_allowlisted(self) -> None:
        regression = {
            "probeId": "privacy-regression",
            "tool": "opcclassic.da.read_sync",
            "errorCode": "EXPECTED_ITEM_MISSING",
            "expectedOutcome": "PASS",
            "expectationFailures": [
                "Expected item 'Plant.Secret.Item' was not returned.",
            ],
            "error": r"Read failed under C:\private\customer-site",
            "path": r"C:\private\customer-site\capture.bin",
            "itemId": "Plant.Secret.Item",
            "count": 2,
            "actual": {
                "outcome": "failure",
                "count": 1,
                "minimumCount": 2,
                "path": r"C:\private\customer-site\capture.bin",
                "expectationFailures": ["Plant.Secret.Item"],
            },
            "futureSensitiveField": "Plant.Secret.Item",
        }
        entry = {
            "profile": "testserver",
            "regressions": [regression],
        }

        safe = run_cross_impl_matrix.persisted_matrix_entry(entry, False)
        sensitive = run_cross_impl_matrix.persisted_matrix_entry(entry, True)
        safe_regression = safe["regressions"][0]

        self.assertEqual(safe_regression["probeId"], "privacy-regression")
        self.assertEqual(safe_regression["count"], 2)
        self.assertEqual(safe_regression["actual"]["count"], 1)
        self.assertEqual(safe_regression["actual"]["minimumCount"], 2)
        self.assertNotIn("expectationFailures", safe_regression)
        self.assertNotIn("error", safe_regression)
        self.assertNotIn("path", safe_regression)
        self.assertNotIn("itemId", safe_regression)
        self.assertNotIn("futureSensitiveField", safe_regression)
        self.assertNotIn("path", safe_regression["actual"])
        self.assertNotIn("expectationFailures", safe_regression["actual"])
        safe_json = json.dumps(safe)
        self.assertNotIn("Plant.Secret", safe_json)
        self.assertNotIn(r"C:\\private", safe_json)
        self.assertEqual(
            sensitive["regressions"][0]["expectationFailures"][0],
            "Expected item 'Plant.Secret.Item' was not returned.",
        )
        self.assertEqual(
            sensitive["regressions"][0]["path"],
            r"C:\private\customer-site\capture.bin",
        )

    def test_testserver_preflight_derives_verified_registration_root(self) -> None:
        executable = self.scratch / "OpcTestServer_x64.exe"
        executable.write_bytes(b"test")
        target = run_cross_impl_matrix.PROFILE_TARGETS["testserver"]

        with patch.object(
            run_cross_impl_matrix,
            "verified_local_server_path",
            return_value=str(executable),
        ):
            roots = run_cross_impl_matrix.descriptor_roots_for_profile(
                "testserver",
                target,
                {},
            )

        self.assertEqual(
            roots["OPC_TESTSERVER_INSTALL_ROOT"],
            str(self.scratch),
        )

    def test_testserver_matrix_preflight_does_not_block_verified_install(self) -> None:
        executable = self.scratch / "OpcTestServer_x64.exe"
        executable.write_bytes(b"test")
        completed = SimpleNamespace(stdout="[]", returncode=0)

        with (
            patch.dict(os.environ, {}, clear=True),
            patch.object(
                run_cross_impl_matrix,
                "verified_local_server_path",
                return_value=str(executable),
            ),
            patch.object(
                run_cross_impl_matrix,
                "is_profile_server_available",
                return_value=(True, None),
            ),
            patch.object(
                run_cross_impl_matrix.subprocess,
                "run",
                return_value=completed,
            ) as execute,
        ):
            result = run_cross_impl_matrix.run_profile(
                matrix_args(),
                "testserver",
                {},
            )

        self.assertFalse(result.get("skipped", False))
        execute.assert_called_once()
        command = execute.call_args.args[0]
        self.assertIn("--probe-workflow", command)
        selected_count = len(vendor.selected_catalog_probes(
            run_cross_impl_matrix.PROFILE_TARGETS["testserver"]["descriptor"],
        ))
        self.assertEqual(len(result["raw_results"]), selected_count)

    def test_missing_testserver_install_blocks_each_selected_probe(self) -> None:
        with (
            patch.dict(os.environ, {}, clear=True),
            patch.object(
                run_cross_impl_matrix,
                "verified_local_server_path",
                return_value=None,
            ),
            patch.object(run_cross_impl_matrix.subprocess, "run") as execute,
        ):
            result = run_cross_impl_matrix.run_profile(
                matrix_args(),
                "testserver",
                {},
            )

        descriptor = run_cross_impl_matrix.PROFILE_TARGETS["testserver"]["descriptor"]
        self.assertTrue(result["skipped"])
        self.assertEqual(
            len(result["raw_results"]),
            len(vendor.selected_catalog_probes(descriptor)),
        )
        self.assertEqual(
            {row["probeId"] for row in result["raw_results"]},
            {probe["id"] for probe in vendor.selected_catalog_probes(descriptor)},
        )
        execute.assert_not_called()

    def test_self_hosted_workflow_exports_verified_testserver_root(self) -> None:
        workflow = (REPO / ".github" / "workflows" / "docker-test-fleet.yml").read_text(
            encoding="utf-8"
        )
        project = (
            REPO
            / "interop"
            / "tools"
            / "vendor-descriptors"
            / "Opc.Classic.VendorDescriptors.csproj"
        ).read_text(encoding="utf-8")

        self.assertIn("OPC_TESTSERVER_INSTALL_ROOT=", workflow)
        self.assertIn("$env:GITHUB_ENV", workflow)
        self.assertIn("<IsPackable>false</IsPackable>", project)

    def test_generic_catalog_has_no_product_binaries(self) -> None:
        _, descriptor = vendor.load_descriptor("generic-opc-classic-template")
        self.assertEqual(descriptor["vendor"], "Operator supplied vendor")
        self.assertFalse(
            any(
                path.suffix.lower() in {".exe", ".dll", ".msi"}
                for path in vendor.DESCRIPTORS.rglob("*")
            )
        )


if __name__ == "__main__":
    unittest.main(verbosity=2)
