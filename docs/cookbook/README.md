# OPC Classic cookbook

Updated for Opc.Classic 0.4.0-alpha.1.

These articles use the current `Opc.Classic.*` package and namespace names and assume the repository is MIT-licensed.

## Articles

- [Matrikon from Linux](01-connect-to-matrikon-from-linux.md): NTLMv2, packet integrity, DA subscription, firewall notes, and pointers to `samples\Opc.Classic.Samples.DaServer`, `samples\Opc.Classic.Samples.AotCanary`, and the managed CTT DA sample.
- [Managed DA server for Windows clients](02-host-managed-da-server-consumed-by-windows-client.md): current `AddClassicServer` / `AddOpcDaServer<T>` hosting flow, aligned with `samples\Opc.Classic.Samples.DaServer`.
- [Kerberos in Active Directory](03-kerberos-in-active-directory.md): Kerberos/SPNEGO, DCOM SPN, EPA, and diagnostics for enterprise auth.
- [Migrate from OPC NET API](04-migrate-from-net-framework-opc-net-api.md): project, package, connection, subscription, and generated-proxy naming changes.
- [DCOM hardening / PKT_INTEGRITY](05-dcom-hardening-pkt-integrity-explainer.md): rationale, overhead, downgrade risk, NTLMv2, and EPA.

## Sample apps and conformance servers referenced

- `samples\Opc.Classic.Samples.DaServer` — managed DA sample server registered as `Opc.Classic.Samples.DaServer.1`.
- `samples\Opc.Classic.Samples.AeServer` — managed AE sample server registered as `Opc.Classic.Samples.AeServer.1`.
- `samples\Opc.Classic.Samples.HdaServer` — managed HDA sample server registered as `Opc.Classic.Samples.HdaServer.1`.
- `samples\Opc.Classic.Samples.AotCanary` — NativeAOT smoke sample for the core/DA/NDR surface.
- `samples\Opc.Classic.Samples.CttServer` — managed DA CTT sample server registered as `Opc.Classic.DaSample.1`.
- `COM\Sample Server\Da`, `COM\Sample Server\Ae`, and `COM\Sample Server\Hda` — OPC Foundation native sample servers used by the Windows compatibility matrix.

See also [../ARCHITECTURE.md](../ARCHITECTURE.md).