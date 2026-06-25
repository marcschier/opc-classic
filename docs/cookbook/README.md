# OPC Classic cookbook

These recipes use the current `Opc.Classic.*` package and namespace names. They are plain GitHub-rendered Markdown and assume the MIT-licensed source tree.

## Recipes

- [Connect Matrikon OPC Explorer to the Linux simulation server](01-connect-to-matrikon-from-linux.md): native Windows client to Linux-hosted authenticated DCOM simulation flow.
- [Host an authenticated managed DA server for Windows clients](02-host-managed-da-server-consumed-by-windows-client.md): `SimulationActivationHost`, server-side NTLM, EPM/135, activation, and callbacks.
- [Kerberos in Active Directory](03-kerberos-in-active-directory.md): SPNs, Kerberos/SPNEGO, channel binding, and diagnostics.
- [Adopt Opc.Classic in OPC NET API projects](04-migrate-from-net-framework-opc-net-api.md): current type and API mappings from OPC NET API concepts to `Opc.Classic.*`.
- [DCOM hardening and packet integrity](05-dcom-hardening-pkt-integrity-explainer.md): why `OpcProtectionLevel.Integrity` is the default and when to use `Privacy`.
- [XML-DA client flows](06-xmlda-client-flows.md): `GetStatus`, read/write calls, and polled subscriptions over HTTP/SOAP.
- [Enabling packet privacy](07-enabling-packet-privacy.md): DCOM `Privacy`, XML-DA over HTTPS, SMB signing, and sample-default caveats.
- [Implementing OPC Security](08-implementing-opc-security.md): server-side `IOPCSecurityNT` / `IOPCSecurityPrivate` wiring and production ACL guidance.
- [Authenticated DCOM server for native OPC clients](09-authenticated-dcom-server-for-native-clients.md): credential model, ports/firewall, activation-to-subscription flow, and troubleshooting.

## Sample apps referenced

- `Opc.Classic.Samples.DaServer` — managed DA server registered as `Opc.Classic.Samples.DaServer.1`.
- `Opc.Classic.Samples.OpcSecurityServer` — DA reference server that publishes OPC Security 1.00 interfaces.
- `Opc.Classic.Samples.DaClient` — DA client flow with browse, read, subscription, and generated proxy wiring.
- `Opc.Classic.Samples.AeServer` — managed AE sample server.
- `Opc.Classic.Samples.AeClient` — AE client/subscription flow.
- `Opc.Classic.Samples.HdaServer` — managed HDA sample server.
- `Opc.Classic.Samples.HdaClient` — HDA query/playback client flow.
- `Opc.Classic.Samples.LoopbackDemo` — in-memory generated proxy/dispatcher loopback.
- `Opc.Classic.Samples.CttServer` — additional managed DA sample registered as `Opc.Classic.DaSample.1` (different CLSID from `samples-da`).
- `Opc.Classic.Samples.SimulationServer` — full-feature simulation server across OPC Classic feature areas.
- `Opc.Classic.Samples.AotCanary` — NativeAOT publish smoke test.

See also [Architecture](../ARCHITECTURE.md), [Adoption guide](../ADOPTION.md) and [sample Docker guide](../../samples/README.docker.md).
