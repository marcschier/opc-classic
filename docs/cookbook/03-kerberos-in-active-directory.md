# Kerberos authentication in an Active Directory environment

Updated for Opc.Classic 0.4.0-alpha.1.

## What this covers

Use Kerberos through SPNEGO for enterprise DCOM authentication when the OPC client and server are in Active Directory.

## Status / availability

`OpcConnectData.WithKerberos`, `OpcAuthMode.Kerberos`, Kerberos/SPNEGO token plumbing, and the `IAuthContext` seam are in the Opc.Classic tree. External Active Directory validation remains part of the 1.0.0 compatibility gate; test the exact SPN, DNS, clock, and packet-integrity policy used by your deployment.

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

SPNEGO wraps the Kerberos AP-REQ in an Init/Resp blob and places it in the DCE/RPC bind auth verifier. Generated DA shims still use `ICallChannel`; only auth changes.

```csharp
var bindOptions = new SpnegoBindOptions
{
    Mechanism = SpnegoMechanism.Kerberos,
    InitialToken = apReq,
    ProtectionLevel = connectData.ProtectionLevel,
};
```

## Channel binding / EPA

For TLS-protected DCOM endpoints, channel binding / EPA includes the `tls-server-end-point` certificate hash in the Kerberos authenticator checksum before SPNEGO wrapping.

## Diagnostics

`LogHost.ConfigureFactory` is in `src\Opc.Classic.Dcom\Internal\LogHost.cs`.

```csharp
using ILoggerFactory loggerFactory = LoggerFactory.Create(b =>
    b.AddConsole().SetMinimumLevel(LogLevel.Trace));
LogHost.ConfigureFactory(loggerFactory);
```

If negotiation fails, check SPN, DNS, clock skew, and packet-integrity policy. See [05-dcom-hardening-pkt-integrity-explainer.md](05-dcom-hardening-pkt-integrity-explainer.md).