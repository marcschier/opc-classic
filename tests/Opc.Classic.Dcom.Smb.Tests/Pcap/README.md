<!-- Copyright (c) 2026 Opc.Classic Contributors. Licensed under the MIT License. -->

# SMB2 PCAP replay fixtures

Drop classic libpcap captures in `Fixtures\` with a `.pcap` extension, or use the committed readable golden fixture `Fixtures\negotiate-smb2-1.txt` as a template. The reader supports basic libpcap (`0xA1B2C3D4`) with `LINKTYPE_ETHERNET` or `LINKTYPE_NULL`, TCP/IP traffic on port 445, and strips the NetBIOS-over-TCP header before replaying SMB2 messages.

Readable golden fixtures may use `.txt` instead:

```text
client -> FE 53 4D 42 ...
server -> FE 53 4D 42 ...
```

Use one SMB2 packet per line. `client`, `client-to-server`, or `c2s` mark client packets; `server`, `server-to-client`, or `s2c` mark server packets. Hex may contain spaces, commas, colons, or underscores. The bytes should normally be post-NetBIOS-framing SMB2 payloads; a leading `00 xx xx xx` NetBIOS length header is also accepted and stripped.

Add a TUnit test that derives from `PcapFixtureBase`, then run:

```powershell
dotnet run --project tests\Opc.Classic.Dcom.Smb.Tests
```
