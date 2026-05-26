# SMB2 wire-format fixtures

This directory's tests validate the SMB2 codec byte-for-byte against the
published Microsoft Open Specifications. Two kinds of fixtures are used:

## (1) Synthetic fixtures — `Smb2WireFixtureTests.cs`

Hand-built byte sequences derived from the field layout tables in
`External/Docs/Win/[MS-SMB2].md` and `External/Docs/Win/[MS-CIFS].md`. Each
test asserts that the codec produces (or accepts) the exact byte layout
documented by the spec. These tests have no external infrastructure
dependency and run on every CI matrix entry (Ubuntu / macOS / Windows).

## (2) PCAP-derived fixtures — capture and replay

For end-to-end interop assurance, real-network captures are extracted from
Wireshark `.pcapng` files into hex byte arrays and baked into test resources.
The capture procedure is below; the resulting fixtures live in
`tests/Opc.Classic.Dcom.Smb.Tests/Fixtures/`.

### Capture (one-time, per server vendor)

1. On a developer Windows VM, install Wireshark and start a capture filtered
   to `tcp.port == 445 and not tcp.analysis.duplicate_ack`.
2. From the same machine, run the existing managed
   `Opc.Classic.Dcom.Smb` smoke tests pointed at the SUT (Samba container or
   Windows VM).
3. Stop the capture once a full NEGOTIATE → SESSION_SETUP → TREE_CONNECT →
   CREATE → IOCTL → CLOSE → LOGOFF cycle is recorded.
4. In Wireshark, select each SMB2 PDU, right-click → "Export Packet Bytes"
   → save as `negotiate-request.bin`, `negotiate-response.bin`, etc.

### Bake into the test project

For each captured `.bin` file, add it as an `EmbeddedResource` in
`Opc.Classic.Dcom.Smb.Tests.csproj` and load via
`Assembly.GetExecutingAssembly().GetManifestResourceStream(...)` in the
relevant test.

A future helper (`Smb2WireFixtures.LoadCaptured("negotiate-response.bin")`)
will normalize the resource-loading boilerplate; until the first real capture
is added it stays unwritten.

### Privacy / redaction notes

Wireshark captures contain:

- The full NTLMSSP Type-1/2/3 messages (server challenge + client response
  + MIC). Treat as session secrets even though no passwords are recoverable;
  use a dedicated test account.
- The OEM hostname and NetBIOS domain in the Type-2 message. Strip with
  Wireshark's editcap if the test will be redistributed.

## Why both?

Synthetic fixtures lock in the **spec** layout — if the spec changes, our
codec needs to follow. PCAP fixtures lock in **real-server compatibility** —
some Windows servers emit fields that the spec allows but doesn't require;
the captures catch them. Phase 6 of the SMB workstream produces the first
captured fixtures alongside the Phase 3 real-server smoke tests.
