#!/usr/bin/env python3
"""Unit tests for tools/probe_matrix.py.

Run with: python tools/test_probe_matrix.py

Exits non-zero on the first failure; uses unittest under the hood so the
output is the standard `OK` / `FAILED (failures=N)` summary.
"""

from __future__ import annotations

import os
import sys
import unittest

# Make sibling probe_matrix.py importable when run from any CWD.
_HERE = os.path.dirname(os.path.abspath(__file__))
if _HERE not in sys.path:
    sys.path.insert(0, _HERE)

import probe_matrix
import probe_servers


_MCP_TOOL_REGEX = __import__("re").compile(r'McpServerTool\(Name = "([^"]+)"')


def _read_live_tool_names(tools_dir: str) -> set[str]:
    """Scan every .cs file in tools_dir for [McpServerTool(Name="..."] and
    return the set of unique tool names."""
    import glob
    names: set[str] = set()
    for path in glob.glob(os.path.join(tools_dir, "*.cs")):
        with open(path, "r", encoding="utf-8") as fh:
            text = fh.read()
        for match in _MCP_TOOL_REGEX.finditer(text):
            names.add(match.group(1))
    return names


class MatrixSmokeTests(unittest.TestCase):
    def test_known_profiles_are_listed_alphabetically(self) -> None:
        names = probe_matrix.known_profile_names()
        self.assertEqual(names, sorted(names))
        # Every documented profile must be exposed.
        for required in (
            "testserver",
            "matrikon",
            "samples-da",
            "samples-ae",
            "samples-hda",
            "ctt-da",
            "security-da",
        ):
            self.assertIn(required, names)

    def test_testserver_profile_marks_iopcitemio_as_pass(self) -> None:
        # TestServer (per our divergence from upstream) advertises both
        # CATID_OPCDAServer20 AND CATID_OPCDAServer30 — so it implements
        # IOPCItemIO and read_items_by_id should succeed.
        expected, verdict = probe_matrix.classify(
            "testserver", "opcclassic.da.read_items_by_id", success=True)
        self.assertEqual(expected, "PASS")
        self.assertEqual(verdict, "MATCH")

    def test_testserver_regression_when_da3_tool_fails(self) -> None:
        # If TestServer regresses and IOPCItemIO stops working, the
        # matrix should flag it.
        expected, verdict = probe_matrix.classify(
            "testserver", "opcclassic.da.read_items_by_id", success=False)
        self.assertEqual(expected, "PASS")
        self.assertEqual(verdict, "REGRESSION")

    def test_matrikon_profile_marks_iopcitemio_as_pass(self) -> None:
        # Matrikon is DA 3.0 capable -- IOPCItemIO should pass.
        expected, verdict = probe_matrix.classify(
            "matrikon", "opcclassic.da.read_items_by_id", success=True)
        self.assertEqual(expected, "PASS")
        self.assertEqual(verdict, "MATCH")

    def test_matrikon_pass_tool_fails_is_regression(self) -> None:
        expected, verdict = probe_matrix.classify(
            "matrikon", "opcclassic.da.get_status", success=False)
        self.assertEqual(expected, "PASS")
        self.assertEqual(verdict, "REGRESSION")

    def test_capture_tools_pass_when_npcap_available(self) -> None:
        for tool in probe_matrix.CAPTURE_TOOLS:
            with self.subTest(tool=tool):
                expected, verdict = probe_matrix.classify(
                    "testserver", tool, success=True, npcap_available=True)
                self.assertEqual(expected, "PASS")
                self.assertEqual(verdict, "MATCH")

                expected, verdict = probe_matrix.classify(
                    "testserver", tool, success=False, npcap_available=True)
                self.assertEqual(expected, "PASS")
                self.assertEqual(verdict, "REGRESSION")

    def test_capture_tools_expected_fail_when_npcap_unavailable(self) -> None:
        for tool in probe_matrix.CAPTURE_TOOLS:
            with self.subTest(tool=tool):
                expected, verdict = probe_matrix.classify(
                    "testserver", tool, success=False, npcap_available=False)
                self.assertEqual(expected, "EXPECTED_FAIL")
                self.assertEqual(verdict, "MATCH")

    def test_session_tools_stay_pass_when_npcap_unavailable(self) -> None:
        expected, verdict = probe_matrix.classify(
            "testserver", "opcclassic.session.create", success=False, npcap_available=False)
        self.assertEqual(expected, "PASS")
        self.assertEqual(verdict, "REGRESSION")

    def test_annotate_uses_npcap_availability_for_capture_only(self) -> None:
        results = [
            {"tool": "opcclassic.capture.start", "success": False, "args": {}, "error": "Unable to load DLL 'wpcap'", "summary": ""},
            {"tool": "opcclassic.session.create", "success": False, "args": {}, "error": "boom", "summary": ""},
        ]
        probe_matrix.annotate(results, "testserver", npcap_available=False)
        self.assertEqual(results[0]["expectedOutcome"], "EXPECTED_FAIL")
        self.assertEqual(results[0]["verdict"], "MATCH")
        self.assertEqual(results[1]["expectedOutcome"], "PASS")
        self.assertEqual(results[1]["verdict"], "REGRESSION")

    def test_npcap_detection_uses_list_interfaces_wpcap_failure(self) -> None:
        results = [
            {"tool": "opcclassic.capture.list_interfaces", "success": False, "args": {}, "error": "Unable to load DLL 'wpcap'", "summary": ""},
        ]
        self.assertFalse(probe_servers.npcap_available_from_results(results))

    def test_npcap_detection_defaults_available_for_other_failures(self) -> None:
        results = [
            {"tool": "opcclassic.capture.list_interfaces", "success": False, "args": {}, "error": "network timeout", "summary": ""},
        ]
        self.assertTrue(probe_servers.npcap_available_from_results(results))

    def test_hda_tool_against_da_server_is_not_applicable(self) -> None:
        expected, verdict = probe_matrix.classify(
            "testserver", "opcclassic.hda.connect", success=False)
        self.assertEqual(expected, "NOT_APPLICABLE")
        self.assertEqual(verdict, "MATCH")

    def test_security_da_profile_marks_security_tools_as_pass(self) -> None:
        # Our OpcSecurityServer sample implements IOPCSecurityNT.
        expected, verdict = probe_matrix.classify(
            "security-da", "opcclassic.security.logon", success=True)
        self.assertEqual(expected, "PASS")
        self.assertEqual(verdict, "MATCH")

    def test_matrix_covers_every_live_mcp_tool(self) -> None:
        # Read every McpServerTool name actually exposed by the MCP server
        # and assert that every profile classifies every tool. This is the
        # safety net that catches matrix drift when a new tool is added.
        tools_dir = os.path.abspath(os.path.join(
            os.path.dirname(__file__), "..", "mcp", "Opc.Classic.Mcp", "Tools"))
        live_tools = _read_live_tool_names(tools_dir)
        self.assertTrue(live_tools, "Expected at least one live MCP tool name")
        for profile in probe_matrix.known_profile_names():
            matrix = probe_matrix.PROFILES[profile]
            missing = sorted(live_tools - set(matrix.keys()))
            self.assertFalse(
                missing,
                f"profile '{profile}' is missing classifications for: {missing}")

    def test_unknown_profile_yields_missing_classification(self) -> None:
        expected, verdict = probe_matrix.classify(
            "no-such-profile", "opcclassic.da.get_status", success=True)
        self.assertEqual(expected, "MISSING_CLASSIFICATION")
        self.assertEqual(verdict, "MISSING_CLASSIFICATION")

    def test_unknown_tool_in_known_profile_yields_missing_classification(self) -> None:
        expected, verdict = probe_matrix.classify(
            "testserver", "opcclassic.never.heard.of.it", success=True)
        self.assertEqual(expected, "MISSING_CLASSIFICATION")
        self.assertEqual(verdict, "MISSING_CLASSIFICATION")

    def test_annotate_writes_columns_in_place(self) -> None:
        results = [
            {"tool": "opcclassic.da.get_status", "success": True, "args": {}, "error": None, "summary": "ok"},
            # security-da profile classifies hda.connect as NOT_APPLICABLE.
            # Use that to exercise the EXPECTED_FAIL/NOT_APPLICABLE annotation.
            {"tool": "opcclassic.hda.connect", "success": False, "args": {}, "error": "wrong spec", "summary": ""},
        ]
        probe_matrix.annotate(results, "security-da")
        self.assertEqual(results[0]["expectedOutcome"], "PASS")
        self.assertEqual(results[0]["verdict"], "MATCH")
        self.assertEqual(results[1]["expectedOutcome"], "NOT_APPLICABLE")
        self.assertEqual(results[1]["verdict"], "MATCH")

    def test_annotate_noop_when_profile_is_none(self) -> None:
        results = [{"tool": "opcclassic.da.get_status", "success": True, "args": {}, "error": None, "summary": "ok"}]
        probe_matrix.annotate(results, None)
        # Should not add the columns when no profile is supplied.
        self.assertNotIn("expectedOutcome", results[0])
        self.assertNotIn("verdict", results[0])

    def test_has_regressions_detects_pass_expected_failures(self) -> None:
        results = [
            {"tool": "opcclassic.da.get_status", "success": False, "args": {}, "error": "x", "summary": ""},
        ]
        probe_matrix.annotate(results, "testserver")
        self.assertTrue(probe_matrix.has_regressions(results))

    def test_has_regressions_false_when_only_expected_fail(self) -> None:
        # Security tools against the bare testserver profile are
        # EXPECTED_FAIL; failures match expectation.
        results = [
            {"tool": "opcclassic.security.logon", "success": False, "args": {}, "error": "E_NOINTERFACE", "summary": ""},
        ]
        probe_matrix.annotate(results, "testserver")
        self.assertFalse(probe_matrix.has_regressions(results))

    def test_summarize_verdicts_counts_all_verdict_kinds(self) -> None:
        results = [
            {"tool": "opcclassic.da.get_status", "success": True, "args": {}, "error": None, "summary": ""},
            {"tool": "opcclassic.security.logon", "success": False, "args": {}, "error": "E_NOINTERFACE", "summary": ""},
            {"tool": "opcclassic.hda.connect", "success": False, "args": {}, "error": "wrong spec", "summary": ""},
        ]
        probe_matrix.annotate(results, "testserver")
        summary = probe_matrix.summarize_verdicts(results)
        # All three are MATCH for testserver: PASS+success, EXPECTED_FAIL+fail,
        # NOT_APPLICABLE+fail.
        self.assertEqual(summary.get("MATCH"), 3)


if __name__ == "__main__":
    unittest.main(verbosity=2)
