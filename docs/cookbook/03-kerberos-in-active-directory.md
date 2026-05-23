# Kerberos authentication in an Active Directory environment

## What this covers

Use Kerberos through SPNEGO for enterprise DCOM authentication when the OPC client and server are in Active Directory.

## Status / availability

Forward-looking: `OpcConnectData.WithKerberos` and `OpcAuthMode.Kerberos` exist in `src\Opc.Classic.Core`, but Kerberos.NET integration is Phase 3D, SPNEGO is Phase 3E, and channel binding is Phase 3F.

## Request a DCOM service ticket

Use the server FQDN. DCOM commonly authenticates to `RPCSS/<server-fqdn>`.

```bash
dotnet add package Kerberos.NET
```

```csharp
using System.Net;
using Kerberos.NET.Client;
using Kerberos.NET.Credentials;
using Opc.Classic;

var url = OpcUrl.Parse("opcda://opc01.corp.example.com/Matrikon.OPC.Simulation.1");
var credential = new NetworkCredential("alice", password, "CORP");
var connectData = OpcConnectData.WithKerberos(url, credential, OpcProtectionLevel.Integrity);

var krb = new KerberosClient();
await krb.Authenticate(new KerberosPasswordCredential("alice@CORP.EXAMPLE.COM", password));
byte[] apReq = await krb.GetServiceTicket("RPCSS/opc01.corp.example.com", cancellationToken);
```

## Bind through SPNEGO

Phase 3E wraps the Kerberos AP-REQ in a SPNEGO Init/Resp blob and places it in the DCE/RPC bind auth verifier. Generated DA shims still use `ICallChannel`; only auth changes.

```csharp
var bindOptions = new SpnegoBindOptions // planned Phase 3E type
{
    Mechanism = SpnegoMechanism.Kerberos,
    InitialToken = apReq,
    ProtectionLevel = connectData.ProtectionLevel,
};
```

## Channel binding / EPA

For TLS-protected DCOM endpoints, Phase 3F adds EPA by including the `tls-server-end-point` certificate hash in the Kerberos AUTHENTICATOR checksum before SPNEGO wrapping.

## Diagnostics

`LogHost.ConfigureFactory` is in `src\Opc.Classic.Dcom\Internal\LogHost.cs`.

```csharp
using ILoggerFactory loggerFactory = LoggerFactory.Create(b =>
    b.AddConsole().SetMinimumLevel(LogLevel.Trace));
LogHost.ConfigureFactory(loggerFactory);
```

If negotiation fails, check SPN, DNS, clock skew, and packet-integrity policy. See [05-dcom-hardening-pkt-integrity-explainer.md](05-dcom-hardening-pkt-integrity-explainer.md).
