# Copyright (c) 2026 Opc.Classic Contributors. Licensed under the MIT License.
"""Descriptor-driven OPC Classic vendor scenario catalogs."""

from __future__ import annotations

import json
import math
import os
import platform
import re
from pathlib import Path, PurePosixPath
from typing import Any


ROOT = Path(__file__).resolve().parents[1] / "interop" / "tools" / "vendor-descriptors"
DESCRIPTORS = ROOT / "descriptors"
SCHEMA_PATH = ROOT / "schemas" / "vendor-probe-catalog-v1.schema.json"
PROFILE_IDS = {
    "matrikon": "matrikon-opc-simulation-server",
    "testserver": "opc-foundation-testserver",
}
WORKFLOW_REQUIRED_TOOLS = {
    "da-reconnect": (
        "opcclassic.da.connect",
        "opcclassic.da.disconnect",
        "opcclassic.da.get_status",
    ),
    "da-failover": (
        "opcclassic.da.connect",
        "opcclassic.da.disconnect",
        "opcclassic.da.browse",
    ),
}

_ID_PATTERN = re.compile(r"^[a-z0-9]+(?:-[a-z0-9]+)*$")
_HDA_RELATIVE_TIME_PATTERN = re.compile(
    r"^NOW(?:(?P<direction>[+-])(?P<magnitude>[0-9]+)(?P<unit>MS|S|M|H|D|W))?$"
)
_MAXIMUM_DESCRIPTOR_BYTES = 256 * 1024
_SCHEMA: dict[str, Any] | None = None


class VendorDescriptorError(ValueError):
    pass


class _FixtureDecodeError(ValueError):
    def __init__(self, code: str, message: str) -> None:
        super().__init__(message)
        self.code = code


def _pairs(pairs: list[tuple[str, Any]]) -> dict[str, Any]:
    result: dict[str, Any] = {}
    for key, value in pairs:
        if key in result:
            raise VendorDescriptorError(f"Duplicate JSON property '{key}'.")
        result[key] = value
    return result


def _constant(value: str) -> None:
    raise VendorDescriptorError(f"Non-finite JSON number '{value}' is forbidden.")


def load_descriptor_json(text: str, source: str = "<memory>") -> dict[str, Any]:
    if len(text.encode("utf-8")) > _MAXIMUM_DESCRIPTOR_BYTES:
        raise VendorDescriptorError(
            f"Descriptor in {source} exceeds {_MAXIMUM_DESCRIPTOR_BYTES} bytes."
        )
    try:
        descriptor = json.loads(
            text,
            object_pairs_hook=_pairs,
            parse_constant=_constant,
        )
    except (json.JSONDecodeError, RecursionError, VendorDescriptorError) as exception:
        raise VendorDescriptorError(
            f"Invalid descriptor JSON in {source}: {exception}"
        ) from exception
    validate_descriptor(descriptor, source)
    return descriptor


def load_descriptor(profile_or_id: str) -> tuple[str, dict[str, Any]]:
    descriptor_id = PROFILE_IDS.get(profile_or_id, profile_or_id)
    if not isinstance(descriptor_id, str) or not _ID_PATTERN.fullmatch(descriptor_id):
        raise VendorDescriptorError("Descriptor id must be a safe kebab-case token.")
    path = resolve_contained_path(DESCRIPTORS, f"{descriptor_id}.json")
    if not path.is_file():
        raise FileNotFoundError(path)
    descriptor = load_descriptor_json(path.read_text(encoding="utf-8"), str(path))
    if descriptor["id"] != descriptor_id:
        raise VendorDescriptorError("Descriptor id does not match file name.")
    return str(path), descriptor


def _schema() -> dict[str, Any]:
    global _SCHEMA
    if _SCHEMA is None:
        try:
            value = json.loads(
                SCHEMA_PATH.read_text(encoding="utf-8"),
                object_pairs_hook=_pairs,
                parse_constant=_constant,
            )
        except (OSError, json.JSONDecodeError, VendorDescriptorError) as exception:
            raise VendorDescriptorError(
                f"Unable to load vendor descriptor schema: {exception}"
            ) from exception
        if not isinstance(value, dict):
            raise VendorDescriptorError("Vendor descriptor schema must be an object.")
        _SCHEMA = value
    return _SCHEMA


def _resolve_schema_reference(schema: dict[str, Any], reference: str) -> dict[str, Any]:
    if not reference.startswith("#/"):
        raise VendorDescriptorError(f"Unsupported schema reference '{reference}'.")
    value: Any = schema
    for raw_segment in reference[2:].split("/"):
        segment = raw_segment.replace("~1", "/").replace("~0", "~")
        if not isinstance(value, dict) or segment not in value:
            raise VendorDescriptorError(f"Unresolved schema reference '{reference}'.")
        value = value[segment]
    if not isinstance(value, dict):
        raise VendorDescriptorError(f"Schema reference '{reference}' is not an object.")
    return value


def _matches_json_type(value: Any, expected: str) -> bool:
    if expected == "object":
        return isinstance(value, dict)
    if expected == "array":
        return isinstance(value, list)
    if expected == "string":
        return isinstance(value, str)
    if expected == "integer":
        return isinstance(value, int) and not isinstance(value, bool)
    if expected == "number":
        return isinstance(value, (int, float)) and not isinstance(value, bool)
    if expected == "boolean":
        return isinstance(value, bool)
    if expected == "null":
        return value is None
    raise VendorDescriptorError(f"Unsupported JSON schema type '{expected}'.")


def _validate_schema_value(
    value: Any,
    rule: dict[str, Any],
    root_schema: dict[str, Any],
    path: str,
    errors: list[str],
) -> None:
    reference = rule.get("$ref")
    if isinstance(reference, str):
        _validate_schema_value(
            value,
            _resolve_schema_reference(root_schema, reference),
            root_schema,
            path,
            errors,
        )
        return

    expected_type = rule.get("type")
    if expected_type is not None:
        expected_types = (
            expected_type if isinstance(expected_type, list) else [expected_type]
        )
        if not any(
            isinstance(item, str) and _matches_json_type(value, item)
            for item in expected_types
        ):
            errors.append(
                f"{path}: expected JSON type "
                f"{'/'.join(str(item) for item in expected_types)}."
            )
            return

    if "const" in rule and value != rule["const"]:
        errors.append(f"{path}: value does not match the declared constant.")
    if "enum" in rule and value not in rule["enum"]:
        errors.append(f"{path}: value is not in the declared enumeration.")

    if isinstance(value, dict):
        required = rule.get("required", [])
        if not isinstance(required, list):
            raise VendorDescriptorError(f"Schema required list at {path} is invalid.")
        for name in required:
            if isinstance(name, str) and name not in value:
                errors.append(f"{path}.{name}: required property is missing.")

        properties = rule.get("properties", {})
        if not isinstance(properties, dict):
            raise VendorDescriptorError(f"Schema properties at {path} are invalid.")
        if rule.get("additionalProperties") is False:
            for name in value:
                if name not in properties:
                    errors.append(f"{path}.{name}: additional property is forbidden.")
        for name, child in value.items():
            child_rule = properties.get(name)
            if isinstance(child_rule, dict):
                _validate_schema_value(
                    child,
                    child_rule,
                    root_schema,
                    f"{path}.{name}",
                    errors,
                )

    elif isinstance(value, list):
        minimum_items = rule.get("minItems")
        maximum_items = rule.get("maxItems")
        if isinstance(minimum_items, int) and len(value) < minimum_items:
            errors.append(f"{path}: expected at least {minimum_items} items.")
        if isinstance(maximum_items, int) and len(value) > maximum_items:
            errors.append(f"{path}: expected no more than {maximum_items} items.")
        if rule.get("uniqueItems") is True:
            try:
                encoded = [
                    json.dumps(
                        item,
                        sort_keys=True,
                        separators=(",", ":"),
                        allow_nan=False,
                    )
                    for item in value
                ]
                if len(encoded) != len(set(encoded)):
                    errors.append(f"{path}: array items must be unique.")
            except (TypeError, ValueError):
                errors.append(f"{path}: array contains an invalid JSON value.")
        item_rule = rule.get("items")
        if isinstance(item_rule, dict):
            for index, child in enumerate(value):
                _validate_schema_value(
                    child,
                    item_rule,
                    root_schema,
                    f"{path}[{index}]",
                    errors,
                )

    elif isinstance(value, str):
        minimum_length = rule.get("minLength")
        maximum_length = rule.get("maxLength")
        if isinstance(minimum_length, int) and len(value) < minimum_length:
            errors.append(f"{path}: string is shorter than {minimum_length}.")
        if isinstance(maximum_length, int) and len(value) > maximum_length:
            errors.append(f"{path}: string is longer than {maximum_length}.")
        pattern = rule.get("pattern")
        if isinstance(pattern, str) and re.search(pattern, value) is None:
            errors.append(f"{path}: string does not match the declared safe pattern.")

    elif isinstance(value, (int, float)) and not isinstance(value, bool):
        if not math.isfinite(value):
            errors.append(f"{path}: non-finite numbers are forbidden.")
            return
        minimum = rule.get("minimum")
        maximum = rule.get("maximum")
        if isinstance(minimum, (int, float)) and value < minimum:
            errors.append(f"{path}: number is below the declared minimum.")
        if isinstance(maximum, (int, float)) and value > maximum:
            errors.append(f"{path}: number is above the declared maximum.")


def validate_descriptor(descriptor: Any, source: str = "<memory>") -> None:
    if not isinstance(descriptor, dict):
        raise VendorDescriptorError("Descriptor must be an object.")

    errors: list[str] = []
    schema = _schema()
    _validate_schema_value(descriptor, schema, schema, "$", errors)
    if errors:
        raise VendorDescriptorError(
            f"Descriptor '{source}' invalid:\n" + "\n".join(errors)
        )

    capabilities = descriptor["capabilities"]
    fixture_ids: set[str] = set()
    for index, fixture in enumerate(descriptor["fixtures"]):
        fixture_id = fixture["id"]
        if fixture_id in fixture_ids:
            errors.append(f"$.fixtures[{index}].id: duplicate fixture id.")
        fixture_ids.add(fixture_id)
        try:
            fixture_path = resolve_contained_path(DESCRIPTORS, fixture["path"])
            if not fixture_path.is_file():
                errors.append(f"$.fixtures[{index}].path: fixture file is missing.")
        except VendorDescriptorError as exception:
            errors.append(f"$.fixtures[{index}].path: {exception}")

    probe_ids: set[str] = set()
    for index, probe in enumerate(descriptor["probes"]):
        if probe["id"] in probe_ids:
            errors.append(f"$.probes[{index}].id: duplicate probe id.")
        probe_ids.add(probe["id"])
        for capability in probe["requires"]:
            if capability not in capabilities:
                errors.append(
                    f"$.probes[{index}].requires: capability '{capability}' "
                    "is not declared."
                )
        if (
            probe["type"] == "fixture-decode"
            and probe.get("fixtureId") not in fixture_ids
        ):
            errors.append(
                f"$.probes[{index}].fixtureId: fixture decode probe must "
                "reference a declared fixture."
            )

    forbidden = {
        "password",
        "secret",
        "token",
        "payload",
        "binary",
        "base64",
        "command",
        "script",
        "setupcommand",
        "installcommand",
    }

    def validate_safe_content(value: Any, path: str = "$") -> None:
        if isinstance(value, dict):
            for key, child in value.items():
                if key.casefold() in forbidden:
                    errors.append(f"{path}.{key}: sensitive or executable content is forbidden.")
                validate_safe_content(child, f"{path}.{key}")
        elif isinstance(value, list):
            for index, child in enumerate(value):
                validate_safe_content(child, f"{path}[{index}]")
        elif isinstance(value, float) and not math.isfinite(value):
            errors.append(f"{path}: non-finite numbers are forbidden.")

    validate_safe_content(descriptor)

    da = descriptor["arguments"].get("da")
    if da:
        lengths = [
            len(da[key])
            for key in ("itemIds", "clientHandles", "writeValues")
        ]
        if len(set(lengths)) != 1 or not lengths[0]:
            errors.append(
                "$.arguments.da: itemIds, clientHandles, and writeValues "
                "must have identical non-zero lengths."
            )

    if errors:
        raise VendorDescriptorError(
            f"Descriptor '{source}' invalid:\n" + "\n".join(errors)
        )


def selected_catalog_probes(descriptor: dict[str, Any]) -> list[dict[str, Any]]:
    capabilities = set(descriptor["capabilities"])
    return [
        probe
        for probe in descriptor["probes"]
        if set(probe["requires"]) <= capabilities
    ]


def descriptor_execution_plan(descriptor: dict[str, Any]) -> list[dict[str, Any]]:
    plan: list[dict[str, Any]] = []
    for probe in selected_catalog_probes(descriptor):
        probe_type = str(probe.get("type") or "")
        if probe_type == "fixture-decode":
            execution = "fixture"
        elif probe_type in WORKFLOW_REQUIRED_TOOLS:
            execution = "workflow"
        elif isinstance(probe.get("tool"), str):
            execution = "tool"
        else:
            execution = "missing"
        plan.append(
            {
                "probeId": probe["id"],
                "type": probe_type,
                "execution": execution,
                "tool": probe.get("tool"),
                "fixtureId": probe.get("fixtureId"),
            }
        )
    return plan


def selected_probe_scenarios(descriptor: dict[str, Any]) -> list[dict[str, str]]:
    return [
        {"probeId": item["probeId"], "tool": item["tool"]}
        for item in descriptor_execution_plan(descriptor)
        if item["execution"] == "tool"
    ]


def selected_probe_workflows(descriptor: dict[str, Any]) -> list[dict[str, str]]:
    return [
        {"probeId": item["probeId"], "workflow": item["type"]}
        for item in descriptor_execution_plan(descriptor)
        if item["execution"] == "workflow"
    ]


def selected_probe_tools(descriptor: dict[str, Any]) -> list[str]:
    tools = {
        "opcclassic.session.create",
        "opcclassic.session.list",
        "opcclassic.session.close",
    }
    for item in descriptor_execution_plan(descriptor):
        if item["execution"] == "tool":
            tools.add(item["tool"])
        elif item["execution"] == "workflow":
            tools.update(WORKFLOW_REQUIRED_TOOLS[item["type"]])
    return sorted(tools)


def require_finite_numbers(value: Any, path: str = "$") -> None:
    if isinstance(value, float) and not math.isfinite(value):
        raise VendorDescriptorError(f"{path} is non-finite.")
    if isinstance(value, dict):
        for key, child in value.items():
            require_finite_numbers(child, f"{path}.{key}")
    elif isinstance(value, list):
        for index, child in enumerate(value):
            require_finite_numbers(child, f"{path}[{index}]")


def final_probe_arguments(descriptor: dict[str, Any]) -> list[str]:
    arguments = descriptor["arguments"]
    require_finite_numbers(arguments, "$.arguments")
    result: list[str] = []
    da = arguments.get("da")
    if da:
        result += [
            "--da-browse-branch", da["browseBranch"],
            "--da-browse-filter", da["browseFilter"],
            "--da-read-item", da["itemIds"][0],
            "--da-group-name", da["groupName"],
            "--da-group-active", str(da["active"]).lower(),
            "--da-update-rate-ms", str(da["updateRateMs"]),
            "--da-read-from-cache", str(da["fromCache"]).lower(),
            "--da-subscription-from-cache", str(da["fromCache"]).lower(),
            "--da-max-notifications", str(da["maxNotifications"]),
        ]
        for value in da["itemIds"]:
            result += ["--da-item-id", value]
        for value in da["clientHandles"]:
            result += ["--da-client-handle", str(value)]
        for value in da["writeValues"]:
            result += [
                "--da-write-value-json",
                json.dumps(value, separators=(",", ":"), allow_nan=False),
            ]
    ae = arguments.get("ae")
    if ae:
        result += [
            "--ae-source", ae["source"],
            "--ae-condition", ae["condition"],
        ]
    hda = arguments.get("hda")
    if hda:
        result += [
            "--hda-item", hda["itemId"],
            "--hda-start", hda["startTime"],
            "--hda-end", hda["endTime"],
            "--hda-at-time", hda["atTime"],
        ]
    return result


def _hresult(value: Any) -> str | None:
    if isinstance(value, int):
        return f"0x{value & 0xffffffff:08X}"
    if isinstance(value, str):
        try:
            return f"0x{int(value, 0) & 0xffffffff:08X}"
        except ValueError:
            return value.upper()
    return None


def normalize_error_code(error: Any) -> str | None:
    if not isinstance(error, str) or not error.strip():
        return None
    upper = error.upper()
    for code in (
        "TOOL_UNAVAILABLE",
        "PROBE_MAPPING_MISSING",
        "PROBE_RESULT_MISSING",
        "DUPLICATE_PROBE_RESULTS",
        "INSTALL_ROOT_NOT_PROVIDED",
        "INSTALL_ROOT_INVALID",
        "FILE_NOT_FOUND",
        "PATH_ESCAPE",
        "SERVER_NOT_REGISTERED",
        "E_ACCESSDENIED",
        "REGDB_E_CLASSNOTREG",
        "CO_E_SERVER_EXEC_FAILURE",
    ):
        if code in upper:
            return code
    match = re.search(r"0X[0-9A-F]{8}", upper)
    if match:
        return match.group(0)
    match = re.match(r"\s*([A-Z][A-Z0-9_]{2,})\s*:", upper)
    return match.group(1) if match else "PROBE_FAILED"


def classify_probe(
    probe: dict[str, Any],
    success: bool,
    error: str | None = None,
) -> str:
    if not success and error and any(
        failure["code"].casefold() in error.casefold()
        for failure in probe["expectedFailures"]
    ):
        return "BLOCKED"
    expected = probe["expected"]["outcome"]
    if expected == "success":
        return "MATCH" if success else "REGRESSION"
    if expected == "failure":
        return "UNEXPECTED_PASS" if success else "MATCH"
    return "UNEXPECTED_PASS" if success else "NOT_APPLICABLE"


def evaluate_probe_result(
    probe: dict[str, Any],
    row: dict[str, Any],
) -> tuple[str, dict[str, Any]]:
    success = bool(row.get("success"))
    error = row.get("error")
    actual: dict[str, Any] = {
        "outcome": "success" if success else "failure",
    }
    error_code = normalize_error_code(error)
    if error_code:
        actual["errorCode"] = error_code
    verdict = classify_probe(probe, success, error)
    if verdict != "MATCH" or not success:
        return verdict, actual

    expected = probe["expected"]
    failures: list[str] = []
    item_id = expected.get("itemId")
    if item_id is not None:
        values = row.get("result")
        candidates = values if isinstance(values, list) else [values]
        match = next(
            (
                item
                for item in candidates
                if isinstance(item, dict)
                and item.get("itemId", item.get("itemName")) == item_id
            ),
            None,
        )
        actual["itemMatched"] = match is not None
        if match is None:
            failures.append(f"Expected item '{item_id}' was not returned.")
        elif "hResult" in expected:
            actual["itemResult"] = match
            actual["hResult"] = _hresult(match.get("hResult"))
            if actual["hResult"] != _hresult(expected["hResult"]):
                failures.append("hResult mismatch.")
        elif match is not None:
            actual["itemResult"] = match

    minimum = expected.get("minimumCount")
    if minimum is not None:
        value = row.get("result")
        count = len(value) if isinstance(value, (list, dict)) else 0
        actual["count"] = count
        if count < minimum:
            failures.append(f"Expected at least {minimum} results, actual {count}.")

    if failures:
        actual["expectationFailures"] = failures
        return "REGRESSION", actual
    return verdict, actual


def report_metadata(descriptor: dict[str, Any]) -> dict[str, Any]:
    return {
        "descriptorVersion": descriptor["schemaVersion"],
        "descriptorId": descriptor["id"],
        "probeCatalogVersion": "1.0",
        "vendor": descriptor["vendor"],
        "product": descriptor["product"],
        "targetKind": descriptor["target"]["kind"],
        "capabilityIds": list(descriptor["capabilities"]),
        "authMode": "operator-supplied",
        "runnerOperatingSystem": platform.system().lower(),
        "runnerBitness": platform.architecture()[0],
    }


def resolve_contained_path(root: os.PathLike[str] | str, relative_path: str) -> Path:
    if not isinstance(relative_path, str) or not relative_path:
        raise VendorDescriptorError("Path must be a non-empty relative string.")
    pure_path = PurePosixPath(relative_path)
    if (
        pure_path.is_absolute()
        or "\\" in relative_path
        or relative_path.startswith("//")
        or re.match(r"^[A-Za-z]:", relative_path) is not None
        or any(part in ("", ".", "..") for part in pure_path.parts)
    ):
        raise VendorDescriptorError("Path is not a safe relative path.")

    try:
        root_path = Path(root)
        if not root_path.is_absolute():
            raise VendorDescriptorError("Containment root must be absolute.")
        resolved_root = root_path.resolve(strict=False)
        candidate = resolved_root.joinpath(*pure_path.parts).resolve(strict=False)
        candidate.relative_to(resolved_root)
    except (OSError, ValueError) as exception:
        raise VendorDescriptorError("Resolved path escapes the containment root.") from exception
    return candidate


def external_prerequisite_results(
    descriptor: dict[str, Any],
    roots: dict[str, Any],
) -> list[dict[str, Any]]:
    results: list[dict[str, Any]] = []
    metadata = report_metadata(descriptor)
    for prerequisite in descriptor["prerequisites"]:
        artifact = prerequisite.get("artifact")
        if not prerequisite["required"] or not artifact:
            continue

        root_value = roots.get(artifact["rootToken"])
        path: str | None = None
        if root_value is None:
            code = "INSTALL_ROOT_NOT_PROVIDED"
            verdict = "BLOCKED"
        else:
            try:
                root = Path(os.fspath(root_value))
                if not root.is_absolute():
                    raise VendorDescriptorError("Install root must be absolute.")
                resolved = resolve_contained_path(root, artifact["relativePath"])
                path = str(resolved)
                if resolved.is_file():
                    code = None
                    verdict = "MATCH"
                else:
                    code = "FILE_NOT_FOUND"
                    verdict = "BLOCKED"
            except (OSError, TypeError, ValueError, VendorDescriptorError):
                code = (
                    "PATH_ESCAPE"
                    if isinstance(root_value, (str, os.PathLike))
                    and Path(os.fspath(root_value)).is_absolute()
                    else "INSTALL_ROOT_INVALID"
                )
                verdict = "REGRESSION" if code == "PATH_ESCAPE" else "BLOCKED"

        results.append(
            {
                "probeId": prerequisite["id"],
                "expected": {"outcome": "success"},
                "actual": {
                    "outcome": "success" if verdict == "MATCH" else "blocked",
                    "path": path,
                    "code": code,
                },
                "verdict": verdict,
                **metadata,
            }
        )
    return results


def _load_fixture_bytes(fixture: dict[str, Any]) -> bytes:
    try:
        path = resolve_contained_path(DESCRIPTORS, fixture["path"])
        text = "".join(path.read_text(encoding="ascii").split())
    except (OSError, UnicodeError, VendorDescriptorError) as exception:
        raise _FixtureDecodeError("FIXTURE_IO_ERROR", str(exception)) from exception
    if len(text) % 2:
        raise _FixtureDecodeError(
            "FIXTURE_HEX_TRUNCATED",
            "Hex fixture contains a truncated byte.",
        )
    try:
        return bytes.fromhex(text)
    except ValueError as exception:
        raise _FixtureDecodeError("FIXTURE_HEX_INVALID", str(exception)) from exception


def _decode_da_fixture(data: bytes, variant: str) -> dict[str, Any]:
    if variant != "vendor-extension":
        raise _FixtureDecodeError(
            "DA_FIXTURE_UNSUPPORTED",
            f"DA fixture variant '{variant}' has no valid wire representation.",
        )
    if len(data) != 6:
        raise _FixtureDecodeError(
            "DA_EXTENSION_LENGTH_INVALID",
            "DA vendor extension must contain a four-byte signature, tag, and revision.",
        )
    if data[:4] != b"\xDE\xAD\xBE\xEF" or data[4] != 0x7F:
        raise _FixtureDecodeError(
            "DA_EXTENSION_HEADER_INVALID",
            "DA vendor extension signature or tag is invalid.",
        )
    return {
        "decoder": "opc-da-vendor-extension",
        "extensionTag": data[4],
        "revision": data[5],
    }


def _decode_ae_fixture(data: bytes, variant: str) -> dict[str, Any]:
    if variant == "empty":
        if data:
            raise _FixtureDecodeError(
                "AE_EMPTY_FIXTURE_NOT_EMPTY",
                "AE empty condition-state fixture must contain no bytes.",
            )
        return {
            "decoder": "opc-ae-condition-state",
            "empty": True,
        }
    if len(data) < 2 or data[:2] != b"AE":
        raise _FixtureDecodeError(
            "AE_CONDITION_STATE_HEADER_INVALID",
            "AE condition-state fixture is missing its protocol signature.",
        )
    if variant == "standard":
        if len(data) != 6:
            raise _FixtureDecodeError(
                "AE_CONDITION_STATE_LENGTH_INVALID",
                "AE standard condition-state fixture must contain state and quality.",
            )
        return {
            "decoder": "opc-ae-condition-state",
            "state": int.from_bytes(data[2:4], "little"),
            "quality": int.from_bytes(data[4:6], "little"),
        }
    if variant == "vendor-extension":
        if len(data) < 5 or data[2] != 0x7F:
            raise _FixtureDecodeError(
                "AE_EXTENSION_HEADER_INVALID",
                "AE vendor extension tag is invalid.",
            )
        payload_length = int.from_bytes(data[3:5], "little")
        if len(data) != 5 + payload_length:
            raise _FixtureDecodeError(
                "AE_EXTENSION_LENGTH_INVALID",
                "AE vendor extension payload length does not match its header.",
            )
        return {
            "decoder": "opc-ae-condition-state-extension",
            "extensionTag": data[2],
            "payloadLength": payload_length,
        }
    raise _FixtureDecodeError(
        "AE_FIXTURE_UNSUPPORTED",
        f"AE fixture variant '{variant}' is not decodable.",
    )


def _decode_hda_fixture(data: bytes, variant: str) -> dict[str, Any]:
    if len(data) < 4 or data[:3] != b"HDA":
        raise _FixtureDecodeError(
            "HDA_RELATIVE_TIME_HEADER_INVALID",
            "HDA relative-time fixture is missing its protocol signature.",
        )
    expected_marker = 0x01 if variant == "standard" else 0x7F
    if variant not in ("standard", "vendor-extension") or data[3] != expected_marker:
        raise _FixtureDecodeError(
            "HDA_RELATIVE_TIME_MARKER_INVALID",
            "HDA relative-time encoding marker is invalid.",
        )
    try:
        expression = data[4:].decode("ascii")
    except UnicodeDecodeError as exception:
        raise _FixtureDecodeError(
            "HDA_RELATIVE_TIME_TEXT_INVALID",
            "HDA relative-time expression must be ASCII.",
        ) from exception
    match = _HDA_RELATIVE_TIME_PATTERN.fullmatch(expression)
    if match is None:
        raise _FixtureDecodeError(
            "HDA_RELATIVE_TIME_EXPRESSION_INVALID",
            "HDA relative-time expression is invalid.",
        )
    magnitude = match.group("magnitude")
    return {
        "decoder": "opc-hda-relative-time",
        "encodingMarker": data[3],
        "expression": expression,
        "direction": match.group("direction"),
        "magnitude": int(magnitude) if magnitude is not None else 0,
        "unit": match.group("unit"),
    }


def _decode_protocol_fixture(fixture: dict[str, Any], data: bytes) -> dict[str, Any]:
    specification = fixture["specification"]
    variant = fixture["variant"]
    if specification == "da":
        return _decode_da_fixture(data, variant)
    if specification == "ae":
        return _decode_ae_fixture(data, variant)
    if specification == "hda":
        return _decode_hda_fixture(data, variant)
    raise _FixtureDecodeError(
        "FIXTURE_SPECIFICATION_UNSUPPORTED",
        f"Unsupported fixture specification '{specification}'.",
    )


def decode_fixture(descriptor: dict[str, Any], fixture_id: str) -> dict[str, Any]:
    fixture = next(
        (value for value in descriptor["fixtures"] if value["id"] == fixture_id),
        None,
    )
    if fixture is None:
        raise KeyError(fixture_id)

    data = b""
    decoded: dict[str, Any] | None = None
    error: str | None = None
    error_code: str | None = None
    try:
        data = _load_fixture_bytes(fixture)
        decoded = _decode_protocol_fixture(fixture, data)
        decoded_ok = True
    except _FixtureDecodeError as exception:
        decoded_ok = False
        error = str(exception)
        error_code = exception.code

    expected_ok = fixture["expectedDecode"] == "success"
    matched = decoded_ok == expected_ok
    probe_id = next(
        (
            probe["id"]
            for probe in descriptor["probes"]
            if probe.get("fixtureId") == fixture_id
        ),
        None,
    )
    actual: dict[str, Any] = {
        "outcome": "success" if decoded_ok else "failure",
        "byteLength": len(data),
        "length": len(data),
        "errorCode": error_code,
        "error": error,
    }
    if decoded is not None:
        actual["decoded"] = decoded
        actual["decoder"] = decoded["decoder"]
    return {
        "probeId": probe_id,
        "tool": "descriptor.fixture.decode",
        "success": matched,
        "expected": {
            "outcome": fixture["expectedDecode"],
            "specification": fixture["specification"],
            "variant": fixture["variant"],
            "redistributable": fixture["redistributable"],
        },
        "actual": actual,
        "verdict": "MATCH" if matched else "REGRESSION",
        **report_metadata(descriptor),
    }
