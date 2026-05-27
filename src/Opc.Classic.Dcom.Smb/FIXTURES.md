# SMB2 wire-format fixtures

This directory's tests validate the SMB2 codec byte-for-byte against the published Microsoft Open Specifications and captured Windows/WINREG traffic. Two kinds of fixtures are used:

## (1) Synthetic fixtures — `Smb2WireFixtureTests.cs`

Hand-built byte sequences derived from the field layout tables in `External\Docs\Win\[MS-SMB2].md` and `External\Docs\Win\[MS-CIFS].md`. Each test asserts that the codec produces (or accepts) the exact byte layout documented by the spec. These tests have no external infrastructure dependency and run on every CI matrix entry (Ubuntu / macOS / Windows).

## (2) Captured WINREG fixtures — capture and replay

Captured DCE/RPC-over-named-pipe bytes live under `tests\Opc.Classic.Dcom.Smb.Tests\Fixtures\Winreg\`:

- `bind_response.bin`
- `openlocalmachine_request.bin`
- `openlocalmachine_response.bin`
- `enumkey_request.bin`
- `enumkey_response.bin`

`WinregFixtureReplayTests` loads these files through `MockWinregServer.ReadFixture(...)`, replays the bind/request/response sequence, canonicalizes call IDs and NDR referents, and asserts that the managed `RegistryStub` marshals/unmarshals the same bytes.

### Capture (one-time, per server vendor)

1. On a developer Windows VM, install Wireshark and start a capture filtered to `tcp.port == 445 and not tcp.analysis.duplicate_ack`.
2. From the same machine, run the managed `Opc.Classic.Dcom.Smb` smoke tests pointed at the SUT (Samba container or Windows VM).
3. Stop the capture once a full NEGOTIATE → SESSION_SETUP → TREE_CONNECT → CREATE → IOCTL → CLOSE → LOGOFF cycle is recorded.
4. In Wireshark, select each SMB2 or DCE/RPC PDU, right-click → "Export Packet Bytes" → save as a descriptive `.bin` fixture.

### Bake into the test project

For each captured `.bin` file, place it under the appropriate `tests\Opc.Classic.Dcom.Smb.Tests\Fixtures\...` subfolder and load it with the folder-specific helper (currently `MockWinregServer.ReadFixture(...)`). Keep canonicalization local to the fixture family so volatile fields are explicit in the replay test.

### Privacy / redaction notes

Wireshark captures can contain:

- The full NTLMSSP Type-1/2/3 messages (server challenge + client response + MIC). Treat as session secrets even though no passwords are recoverable; use a dedicated test account.
- The OEM hostname and NetBIOS domain in the Type-2 message. Strip with Wireshark's editcap if the test will be redistributed.

The committed WINREG fixtures are reduced replay payloads used by tests; do not add raw `.pcapng` captures to the repository.

## Why both?

Synthetic fixtures lock in the **spec** layout — if the spec changes, our codec needs to follow. Captured fixtures lock in **real-server compatibility** — some Windows servers emit fields that the spec allows but doesn't require; the captures catch them.
