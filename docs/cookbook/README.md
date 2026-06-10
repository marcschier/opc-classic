# OPC Classic cookbook

These recipes use the current `Opc.Classic.*` package and namespace names. They are plain GitHub-rendered Markdown and assume the MIT-licensed `1.0.0-rc.10` source tree.

## Recipes

- [Connect to Matrikon from Linux](01-connect-to-matrikon-from-linux.md): NTLMv2 or Kerberos, packet integrity, DA reads/subscriptions, and firewall notes.
- [Host a managed DA server for Windows clients](02-host-managed-da-server-consumed-by-windows-client.md): `AddClassicServer`, `AddClassicClsidRegistry`, `AddOpcDaServer<T>`, and native COM activation expectations.
- [Kerberos in Active Directory](03-kerberos-in-active-directory.md): SPNs, Kerberos/SPNEGO, channel binding, and diagnostics.
- [Adopt Opc.Classic in OPC NET API projects](04-migrate-from-net-framework-opc-net-api.md): current type and API mappings from OPC NET API concepts to `Opc.Classic.*`.
- [DCOM hardening and packet integrity](05-dcom-hardening-pkt-integrity-explainer.md): why `OpcProtectionLevel.Integrity` is the default and when to use `Privacy`.
- [XML-DA client flows](06-xmlda-client-flows.md): `GetStatus`, read/write calls, and polled subscriptions over HTTP/SOAP.
- [Enabling packet privacy](07-enabling-packet-privacy.md): DCOM `Privacy`, XML-DA over HTTPS, SMB signing, and sample-default caveats.
- [Implementing OPC Security](08-implementing-opc-security.md): server-side `IOPCSecurityNT` / `IOPCSecurityPrivate` wiring and production ACL guidance.

## Sample apps referenced

- `samples\Opc.Classic.Samples.DaServer` — managed DA server registered as `Opc.Classic.Samples.DaServer.1`.
- `samples\Opc.Classic.Samples.OpcSecurityServer` — DA reference server that publishes OPC Security 1.00 interfaces.
- `samples\Opc.Classic.Samples.DaClient` — DA client flow with browse, read, subscription, and generated proxy wiring.
- `samples\Opc.Classic.Samples.AeServer` — managed AE sample server.
- `samples\Opc.Classic.Samples.AeClient` — AE client/subscription flow.
- `samples\Opc.Classic.Samples.HdaServer` — managed HDA sample server.
- `samples\Opc.Classic.Samples.HdaClient` — HDA query/playback client flow.
- `samples\Opc.Classic.Samples.LoopbackDemo` — in-memory generated proxy/dispatcher loopback.
- `samples\Opc.Classic.Samples.CttServer` — additional managed DA sample registered as `Opc.Classic.DaSample.1` (different CLSID from samples-da).
- `samples\Opc.Classic.Samples.AotCanary` — NativeAOT publish smoke test.

See also [Architecture](../ARCHITECTURE.md), [Adoption guide](../ADOPTION.md) and [sample Docker guide](../../samples/README.docker.md).
