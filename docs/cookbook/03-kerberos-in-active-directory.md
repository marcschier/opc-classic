# Kerberos authentication in an Active Directory environment

## What this covers

Use Kerberos through SPNEGO for enterprise DCOM authentication when the OPC client and server are joined to, or trusted by, Active Directory.

`OpcConnectData.WithKerberos`, `OpcAuthMode.Kerberos`, Kerberos packet protection, SPNEGO token wrapping, and the `IAuthContext` seam are available in the `Opc.Classic.*` tree.

## Service principal

Use the server FQDN. DCOM commonly authenticates to `RPCSS/<server-fqdn>`.

```text
RPCSS/opc01.corp.example.com
```

Ensure DNS, reverse lookup expectations, realm casing, and clock synchronization are correct before testing application code.

## Connection data

```csharp
using System.Net;
using Opc.Classic;

var url = OpcUrl.Parse("opcda://opc01.corp.example.com/Matrikon.OPC.Simulation.1");
var credential = new NetworkCredential("alice@CORP.EXAMPLE.COM", password, "CORP.EXAMPLE.COM");

var connectData = OpcConnectData.WithKerberos(
    url,
    credential,
    OpcProtectionLevel.Integrity,
    operationTimeout: TimeSpan.FromSeconds(30));
```

## SPNEGO

SPNEGO wraps the Kerberos AP-REQ/AP-REP exchange for peers that negotiate through [MS-SPNG] / RFC 4178. Generated DA, AE, and HDA shims still use `ICallChannel`; only the authentication context changes.

Keep NTLMv2 fallback explicit. Some environments disable NTLM entirely, and a silent fallback can violate policy.

## Channel binding / EPA

For TLS-protected DCOM endpoints, channel binding includes the `tls-server-end-point` certificate hash in the authentication exchange.

```csharp
using Opc.Classic.Security;

byte[] serverCertificateDer = GetServerCertificateBytes();
ChannelBindings bindings = ChannelBindingsFactory.ForTlsServerEndpoint(serverCertificateDer);

var connectData = OpcConnectData.WithKerberos(
    url,
    credential,
    OpcProtectionLevel.Integrity,
    channelBindings: bindings);
```

The helper follows RFC 5056 and RFC 5929.

## Diagnostics

If negotiation fails, check:

- KDC reachability;
- DNS and SPN match;
- client/server clock skew;
- account lockout or delegation policy;
- DCOM firewall ports;
- packet-integrity policy;
- whether SPNEGO selected Kerberos or NTLMv2.

Use application logging around the selected `OpcAuthMode` and protection level. For packet-integrity rationale, see [DCOM hardening and packet integrity](05-dcom-hardening-pkt-integrity-explainer.md); for NTLMv2 audit context and residual-risk tracking, see [../security/NTLMSSP_AUDIT_GUIDE.md](../security/NTLMSSP_AUDIT_GUIDE.md).
