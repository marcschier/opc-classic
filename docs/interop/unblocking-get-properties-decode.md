# Unblocking `da.get_properties` / `cpx.get_complex_type` decode failure

This guide walks through the manual capture work needed to close
`ag-get-properties-decode` — the response-side wire-decode issue against
Matrikon Simulation Server's `IOPCBrowse::GetProperties` /
`IOPCItemProperties::GetItemProperties` response payloads.

## Background

The request side was closed by Tracks AF4 + AG1 (commits in the
`dfbf234b`/`3a1ba9c3` lineage). Track AT (`d569f384`) verified the
**OPC DA 3.00 §6.5 standard property set** (PropertyId 1–7: canonical
datatype `VT_I2`, value, quality `VT_I2`, timestamp `VT_FILETIME`,
access rights `VT_I4`, scan rate `VT_R4`, EU type `VT_I4`) round-trips
byte-perfect through the per-element VARIANT codec via 5 synthetic
fixture tests in
[`tests/Opc.Classic.Da.Tests/Wire/GetItemPropertiesStandardSetFixtureTests.cs`](../../tests/Opc.Classic.Da.Tests/Wire/GetItemPropertiesStandardSetFixtureTests.cs).

**If the live Matrikon variant still fails after Tracks AS/AT, the bug
is vendor-specific padding — not spec-compliant layout.** This guide is
the procedure for capturing the failing exchange so the vendor
variation can be diffed against the spec fixtures and a targeted fix
shipped.

Shipped scaffolding that makes this cheap:

- Track AK1 (`89d68772`): every NDR decode-fail throws an
  `InvalidDataException` whose message ends with a hex window centered
  on the failing offset (via `NdrReader.FormatContext()`).
- Track AK2 (`89d68772`): opt-in wire capture via
  `OPCCLASSIC_WIRE_CAPTURE_DIR` env var (or the
  `--save-wire-payloads <dir>` flag on `tools/probe_servers.py`).
- Track AK3 (`8b98cda2`): `WireCaptureFile` parser
  ([`tests/Opc.Classic.Da.Tests/Wire/Replay/WireCaptureFile.cs`](../../tests/Opc.Classic.Da.Tests/Wire/Replay/WireCaptureFile.cs))
  that turns a captured `.hex` file back into a `byte[]` for direct
  codec replay.
- Track AS (`778dbce7`) + Track AW (`4b89811b`): every NDR FILETIME
  decoder now uses `FileTimeHelper.TryFromFileTime` + structured
  `InvalidDataException` with named field, so a sentinel FILETIME from
  Matrikon will be caught with a clean error rather than crash the
  decoder.

## Prerequisites (one-time, ~5 min)

- [ ] Matrikon OPC Simulation Server installed and running on a
      reachable host. (Typically the dev box itself — `localhost`. If
      it's on another machine, jot down the hostname.)
- [ ] OPCEnum service running on the Matrikon host
      (`Get-Service OpcEnum`).
- [ ] Your dev account has DCOM Activation + Access on the Matrikon
      AppID. If it doesn't:

      ```powershell
      # Elevated 64-bit PowerShell on the Matrikon host:
      .\tools\grant-opcenum-acl.ps1
      ```

      (Shipped in Track AN, commit `1a1de5db`. Idempotent; rolls back
      via `-Unregister`. See [`opcenum-auth.md`](opcenum-auth.md) for
      details.)
- [ ] Python 3.10+ with `requests` package: `pip install requests`.
- [ ] `dotnet` 10.0.100+ available in PATH.

## Step 1 — Smoke-test the environment with GetStatus (~2 min)

Verifies your environment can reach Matrikon at all. If this fails the
capture won't be useful — the actual blocker is auth/activation, not
decode.

```powershell
cd <repo>
dotnet build mcp\Opc.Classic.Mcp\Opc.Classic.Mcp.csproj -c Release

# From a clean shell so OPCCLASSIC_WIRE_CAPTURE_DIR isn't sticky:
python tools\probe_servers.py `
  --da-progid Matrikon.OPC.Simulation.1 `
  --auth-level pkt_integrity `
  --request-timeout 30 `
  --probe opcclassic.da.get_status
```

**Expected**: a JSON status block with `state`, `server_version`, etc.

**If you instead get** `rpc_s_access_denied` or
`RPC fault status 0x000006F7`: OPCEnum AppID ACLs are still missing.
Re-run `grant-opcenum-acl.ps1` and verify `Get-Service OpcEnum`. Don't
proceed until GetStatus works.

## Step 2 — Capture the failing `get_properties` exchange (~1 min)

Now the real capture. The MCP server writes one `.hex` file per
`ICallChannel.InvokeAsync` while the env var is set. Each file is
self-describing (timestamp, IID, opnum, request bytes, response bytes,
HRESULT).

```powershell
$captureDir = "docs\interop\wire-captures\matrikon-getprops-$(Get-Date -Format yyyyMMdd-HHmmss)"
New-Item -ItemType Directory -Path $captureDir -Force | Out-Null

python tools\probe_servers.py `
  --da-progid Matrikon.OPC.Simulation.1 `
  --auth-level pkt_integrity `
  --request-timeout 30 `
  --save-wire-payloads $captureDir `
  --probe opcclassic.da.get_properties `
  --da-read-item Random.Int4
```

The probe will fail — that's the point. The `$captureDir` will contain
~10–30 `.hex` files representing every DCOM call leading up to the
failure: BindPdu, AlterContext, `IOPCBrowse::GetProperties`, etc.

## Step 3 — Identify the failing payload (~3 min, purely visual)

```powershell
Get-ChildItem $captureDir | Sort-Object Name |
  ForEach-Object { "{0,-90} {1,8}" -f $_.Name, $_.Length }
```

Each filename encodes
`<timestamp>_<seq>_<context>_iid-<iid>_op-<opnum>.hex`. **Look for the
LAST file** (highest sequence number) where the response payload is
non-empty AND the request was for one of:

- `IOPCBrowse` (IID `39227004-A18F-4B57-8B0A-5235670F4468`) opnum 6 —
  `GetProperties`.
- `IOPCItemProperties` (IID `39C13A72-011E-11D0-9675-0020AFD8ADB3`)
  opnum 4 — `GetItemProperties`.

The file we want will look like:

```text
20260603T182300.456_000019_da-localhost-Matrikon_iid-39227004-a18f-4b57-8b0a-5235670f4468_op-6.hex
```

## Step 4 — Sanity-check the capture file (~30 sec)

Open the `.hex` file in a text editor. The format is human-readable:

```text
# Opc.Classic wire capture
# context: da-localhost-Matrikon.OPC.Simulation.1
# iid:     39227004-a18f-4b57-8b0a-5235670f4468
# opnum:   6
# hresult: 0x00000000
# timestamp_utc: 20260603T182300.456

## request (N bytes)
0000:  02 00 00 00 ...

## response (M bytes)
0000:  00 00 02 00 ...
```

Confirm:

- The response section is present and non-empty (the failure happens
  during DECODE, so the bytes did arrive).
- The `# hresult` line shows `0x00000000` (the call succeeded at the
  RPC layer; the decode is what threw).

**If response is empty or hresult is non-zero**: the failure is not a
decode issue and the `ag-get-properties-decode` todo description is
wrong. Stop and re-triage — likely a server-side issue, an
authorization gap, or an interface QI failure earlier in the exchange.

## Step 5 — Scrub server-identifying data (~1 min, optional)

If the capture contains item names you'd rather not share (e.g. plant
equipment identifiers), open the `.hex` file and `s/SecretItem/ItemXX/g`
on the ASCII gutter rows. The hex columns themselves can stay — they're
just byte values.

For `Random.Int4` (the suggested probe item) there's nothing to scrub.

## Step 6 — Hand off the capture

Submit two things:

1. The contents of the failing `.hex` file (one fenced code block).
2. The MCP server's stderr from Step 2 — the actual exception message,
   which now includes the wire-context hex window thanks to Track AK1.

The next iteration (automated) will:

- Replay the captured response bytes through the spec-compliant codec
  to reproduce the failure offline (via `WireCaptureFile.LoadResponse`).
- Diff against the AT2 synthetic fixtures (which decode cleanly) to
  find the exact byte offset where Matrikon's layout diverges from spec.
- Ship a targeted decode-path fix gated on the observed vendor
  variation (typically: extra padding before a length-prefixed string,
  or a non-canonical `clSize`/`rpcReserved` shape).
- Add the captured file as a permanent regression fixture under
  `docs/interop/wire-captures/`.

## Out of scope (don't do these)

- **Run Wireshark and decode the OPC DCOM frames manually** — the
  wire-capture diagnostic gives the same bytes pre-decryption with full
  IID/opnum/HRESULT metadata. Wireshark adds no information here.
- **Patch the codec based on guesswork** — Track AT proved the spec
  layout decodes correctly; without the failing bytes any change is
  speculative and risks breaking what works.
- **Re-test against a non-Matrikon server** — vendor padding is the
  suspected variable; only a Matrikon capture closes the question. The
  OPC Foundation TestServer paths are already covered by the spec
  fixtures.

## Estimated total elapsed time

| Phase | Time |
|---|---|
| One-time prerequisites | ~5 min |
| Steps 1–6 | ~7–8 min on a working dev box |
| **Total active work** | **~15 min** |

## Related

- [Probe coverage](probe-coverage.md) — full MCP tool-by-tool status
  including the `get_properties` failure mode.
- [Wire captures](wire-captures/README.md) — capture format reference
  and replay helper documentation.
- [OPCEnum DCOM auth](opcenum-auth.md) — `tools/grant-opcenum-acl.ps1`
  helper used in prerequisites.
- [`tests/Opc.Classic.Da.Tests/Wire/GetItemPropertiesStandardSetFixtureTests.cs`](../../tests/Opc.Classic.Da.Tests/Wire/GetItemPropertiesStandardSetFixtureTests.cs)
  — the AT2 synthetic fixtures to diff against.
- [`tests/Opc.Classic.Da.Tests/Wire/Replay/WireCaptureFile.cs`](../../tests/Opc.Classic.Da.Tests/Wire/Replay/WireCaptureFile.cs)
  — the parser that turns a captured `.hex` back into a `byte[]`.
- [OPC DA 3.00 §6.5](../../ext/private/docs/OPC-DA-3.00.md) — the
  `IOPCItemProperties` interface specification.
