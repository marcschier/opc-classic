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

- `IOPCBrowse` (IID `39227004-A18F-4B57-8B0A-5235670F4468`) opnum **3** —
  `GetProperties` (DA 3.0 unified browse).
- `IOPCItemProperties` (IID `39C13A72-011E-11D0-9675-0020AFD8ADB3`)
  opnum **4** — `GetItemProperties` (DA 2.0 property interface).

The file we want will look like:

```text
20260603T182300.456_000019_da-localhost-Matrikon_iid-39227004-a18f-4b57-8b0a-5235670f4468_op-3.hex
```

Quick find via PowerShell:

```powershell
# Pick the most recent GetProperties capture (DA 3.0 path):
Get-ChildItem $captureDir -Filter "*39227004*op-3.hex" |
  Sort-Object Name -Descending | Select-Object -First 1 -ExpandProperty FullName

# Or the DA 2.0 path if the DA 3.0 one is absent:
Get-ChildItem $captureDir -Filter "*39c13a72*op-4.hex" |
  Sort-Object Name -Descending | Select-Object -First 1 -ExpandProperty FullName
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

## Track AY+ — FULLY RESOLVED (Matrikon wire is spec-compliant; our codec had 3 stacking bugs)

**Status: ✅ CLOSED.** The user was right — Matrikon is not incompatible. The
live Matrikon Simulation Server emits a fully spec-compliant
`IOPCBrowse::GetProperties` response per MS-OAUT and DCE/RPC. **Our codec had
three stacking bugs** that all compounded to make the wire look "vendor-shaped":

### Bug 1 — embedded VARIANT was treated as inline struct instead of [unique] pointer

`OPCITEMPROPERTY.vValue` is typed `VARIANT` in the IDL, but MS-OAUT 2.2.29.2
defines `typedef [unique] struct _wireVARIANT * VARIANT` — so it's
fundamentally a unique pointer at NDR level. MIDL emits the field as
`FC_USER_MARSHAL` with flags byte `0x83`: the high nibble `0x80` is
`USER_MARSHAL_UNIQUE`. That means:

- The **inline** OPCITEMPROPERTY part carries just a **4-byte VARIANT
  referent** (alongside vt/wRes/propId/itemIdRef/descRef/hrError/dwReserved =
  28 bytes total per inline part).
- The actual **wireVARIANT body** lives in the **deferred-pile** AFTER
  szItemID and szDescription for each property.

Our codec was reading the wireVARIANT INLINE between the string referents and
the trailing hrError. That over-consumed bytes for every property, causing
catastrophic drift on the second inline part.

### Bug 2 — missing `[switch_type(ULONG)]` discriminator on the wire

MS-OAUT 2.2.29.1 wireVARIANT has:
`[switch_type(ULONG), switch_is(vt)] union { ... } _varUnion;`

Per C706 §14.4.1 (non-encapsulated unions), when `switch_type` differs from
the type of the `switch_is` field — here ULONG vs USHORT — NDR writes the
discriminator **explicitly** on the wire. So a 4-byte ULONG copy of `vt`
sits between the 16-byte wireVARIANT header and the union body.

Our `ReadVariantCore` / `WriteVariantCore` were skipping this discriminator,
causing per-variant 4-byte drift on top of bug 1.

### Bug 3 — FLAGGED_WORD_BLOB (BSTR) missing max_count prefix

MS-OAUT 2.2.23 defines:
```idl
typedef struct _FLAGGED_WORD_BLOB {
    unsigned long cBytes;
    unsigned long clSize;
    [size_is(clSize)] unsigned short asData[];
} FLAGGED_WORD_BLOB;
```

The `[size_is(clSize)] asData[]` conformant array carries an implicit
**max_count prefix** at the START of the struct per NDR rules. So the wire
layout is `referent + max_count + cBytes + clSize + WCHAR[clSize]`.

Our `NdrReader.ReadBstr` / `NdrWriter.WriteBstr` were writing
`referent + fFlags + clSize + chars` (no max_count, and misnaming cBytes as
fFlags). Loopback tests passed because reader and writer were symmetrically
wrong.

### clSize is in quadwords, not bytes

MS-OAUT 2.2.29.1 specifies `clSize` "MUST be set to the size, in quad words
(64 bits), of the structure". Our writer emitted clSize in BYTES. Matrikon
reads clSize as quadwords (rounded up); our spec-correct write now emits
ceiling-clSize and our reader treats clSize as an upper-bound hint (it trusts
header + discriminator + body for actual position advancement, since some
senders, including Matrikon, do not pad the wire to a multiple of 8).

### Verification

The replay test now passes against the live Matrikon Simulation Server
capture (`matrikon-getproperties-random-int4.hex`):

- 14 properties decoded
- vtDataType + dwPropertyID + szItemID + szDescription + value + hrErrorID
  all match OPC DA 3.00 §A.1 standard + recommended set:
  - #1 Item Canonical DataType = VT_I2 holding VT_I4=3
  - #2 Item Value = VT_I4 (random int)
  - #3 Item Quality = VT_I2 = 192 (OPC_QUALITY_GOOD)
  - #4 Item Timestamp = VT_DATE (current timestamp)
  - #5 Item Access Rights = VT_I4 = 1 (OPC_READABLE)
  - #6 Server Scan Rate = VT_R4 = 100
  - #7 Item EU Type = VT_I4 = 0 (OPC_NO_ENUM)
  - #8 Item EUInfo = VT_EMPTY (non-enumerated)
  - #9 Item Description (BSTR)
  - #10-14 Matrikon-private waveform properties

Full solution test sweep: **all green** (0 failures across 17 test projects).

## Track AY — original wire-replay findings (preserved for history)

The first live capture against Matrikon OPC Simulation Server was
landed as
[`tests/Opc.Classic.Da.Tests/Wire/Fixtures/matrikon-getproperties-random-int4.hex`](../../tests/Opc.Classic.Da.Tests/Wire/Fixtures/matrikon-getproperties-random-int4.hex).
The `MatrikonGetPropertiesReplayTests.Replay_decodes_response_through_browse_decoder`
test reproduces the failure deterministically off-line:

```
NDR VARIANT wire decoding is not supported for type 57.
Wire context (bytes 184..216, >> marks position 200):
  00B0:                          55 73 65 72 00 00 00 00          User....
  00C0:  39 00 2e 00 03 00 35 00>>07 00 00 00 38 00 02 00  9.....5.....8...
```

What a programmatic walk of the response payload revealed:

1. **Per-property inline stride is exactly 28 bytes**, not the 40+
   bytes a spec-compliant `OPCITEMPROPERTY` would produce. The
   `"User\0\0\0\0"` 8-byte marker appears at offsets 44, 72, 100, 128,
   156, 184, 212, 240, 268, 296, 324, 352, 380, 408 — once per
   property, exactly 28 bytes apart. Decoding 14 inline parts × 28
   bytes places the deferred conformant string section at offset 432,
   which lines up byte-perfect with `UTF16 @432: Random.Int4`.
2. **`dwPropertyID` values DO check out at 28-byte stride** for the
   first nine properties — 1, 2, 3, 4, 5, 6, 7, 8, 101 (matching the
   OPC DA 3.00 §A.1 standard + recommended set Item Description = 101).
   Properties 10–14 carry the 0xFFFFFFFB…0xFFFFFFFF range (vendor /
   Matrikon-private property IDs for the Random / Triangle / Square /
   Saw / Bucket Brigade simulated waveforms).
3. **The codec drift starts at the very first property**, not deep into
   the array. After reading the 16-byte header (vt, wReserved, propId,
   szItemID ref, szDescription ref) at bytes 28..43, the codec does
   `AlignTo(8)` (consuming "User") then reads a wireVARIANT at offset
   48 — but the bytes there don't shape like a wireVARIANT (clSize=0,
   rpcReserved=0x0031002E, vt=0x0003, but the body decode of 4 bytes
   bleeds into what should be the next property's header). The codec
   "consumes" 48 bytes for the first inline part instead of 28, and
   the 20-byte over-read cascades.
4. **`OPCITEMPROPERTY` IDL is identical** between
   `external/inc/opcda.idl`, `external/inc/opcda.h`, and
   `external/redist/src/DataAccess/ProxyStub/opcda.idl` — so the
   layout difference is not an IDL-level vendor extension. Matrikon
   ships their own proxy/stub DLL alongside the simulation server; the
   wire shape produced by that proxy is what we are observing.
5. **`OPCITEMPROPERTIES` (outer wrapper) wire shape DOES match the
   Foundation custom proxy/stub** — the trailing `dwReserved` DWORD
   the codec already reads (line 117 of `NdrOpcBrowseResponseDecoder`)
   is present at offset 20 and is zero. So the discrepancy is purely
   in the per-element `OPCITEMPROPERTY`, not the array wrapper.

### What is still unknown

The 12-byte trailer per inline (offsets +16..+27 of each 28-byte slot)
encodes:

- 8 bytes of constant `"User\0\0\0\0"` (the 0x72657355 magic) — present
  in every property, regardless of vtDataType or value
- 4 trailing bytes that vary per property (looks like uninitialized
  stack memory or fragments of UTF-16 strings; values like `2E 00 31 00`
  / `36 00 38 00` / `8B 0A 52 35` don't match any obvious VARIANT body
  or HRESULT shape)

There is **no separate deferred VARIANT pile** in the response — the
strings section starts at byte 432 immediately after the 14 × 28-byte
inline parts, and the trailing 32 bytes after the strings (1900..1932)
contain only `00 00 00 00 03 00 00 00 …` patterns that don't fit 14
mixed-type variant bodies.

This shape is **not standard MS-OAUT wireVARIANT** marshalling and is
not produced by stock MIDL. Two plausible next-step hypotheses:

- **Matrikon ships a hand-rolled proxy/stub** that compresses the
  embedded `vValue` into a fixed 12-byte slot (with `"User"` as a
  literal magic tag), trusting the recipient's matching proxy to know
  the convention. Confirming this would require disassembling the
  Matrikon proxy DLL.
- **The `"User"` bytes are leaked stack/heap memory from a marshalling
  buffer that the Matrikon server fills but never zeroes**, and the
  variant body is actually elsewhere (perhaps the per-property
  `hrErrorID`/`dwReserved` are deferred to the trailing 32 bytes after
  the strings). This would explain why
  `"User\0\0\0\0"` is constant across all 14 — it's the same scratch
  buffer reused per property.

### Recommended next step

Run the OPC Foundation's stock reference proxy from an external official Core
Components install against the same Matrikon item and capture the response. If
the wire bytes match what we captured, the layout IS Matrikon's documented proxy
convention and we can model it. If they differ, our managed proxy must be
sending something subtly wrong in the request (e.g. a context ID mismatch)
that's triggering Matrikon to fall back to a custom response shape.

Until that comparison is done, **do not change the codec** — the
in-tree synthetic fixtures all pass and adding a Matrikon-shaped
decoder branch without ground truth risks shipping a guess.

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
- [OPC DA 3.00 §6.5](../../external/private/docs/OPC-DA-3.00.md) — the
  `IOPCItemProperties` interface specification.
