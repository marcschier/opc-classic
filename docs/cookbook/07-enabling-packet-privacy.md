# Enabling packet privacy

## What this covers

Packet privacy means authenticated, signed, and encrypted transport for OPC payloads. `Opc.Classic` keeps packet integrity as the cross-machine default for compatibility, so confidentiality is an explicit production opt-in.

Use privacy for any deployment outside a hardened local-only loopback or disposable interop rig.

## Packages and namespaces

```bash
dotnet add package Opc.Classic.Core
dotnet add package Opc.Classic.Dcom
dotnet add package Opc.Classic.Xml
```

Core types:

- `OpcConnectData`, `OpcProtectionLevel`, `OpcAuthMode`, `OpcClientOptions`, and `IAuthContext` are in `Opc.Classic`.
- `DcomCallChannelFactory.ConnectTcpAsync` is in `Opc.Classic.Dcom.Transport`.
- `NtlmAuthentication.CreateAuthContext` is in `Opc.Classic.Dcom.Rpc.Auth.ntlm`.
- `HttpXmlDaClient` is in `Opc.Classic.Xml`.

Verified source references: `OpcProtectionLevel`, `OpcConnectData`, `OpcAuthMode`, `OpcClientOptions`, and `IAuthContext`. `OpcClientOptions` currently carries operation timeout and circuit-breaker settings; packet protection is configured on `OpcConnectData`.

## DCOM client: select packet privacy

`OpcProtectionLevel.Privacy` maps to `RPC_C_AUTHN_LEVEL_PKT_PRIVACY`.

```csharp
using System.Net;
using Opc.Classic;
using Opc.Classic.Dcom.Rpc.Auth.ntlm;
using Opc.Classic.Dcom.Transport;

OpcUrl url = OpcUrl.Parse("opcda://opc01.corp.example.com/Matrikon.OPC.Simulation.1");
var credentials = new NetworkCredential("opc-reader", password, "CORP");

OpcConnectData connectData = OpcConnectData.WithNtlmV2(
    url,
    credentials,
    protectionLevel: OpcProtectionLevel.Privacy,
    operationTimeout: TimeSpan.FromSeconds(30));

IAuthContext authContext = NtlmAuthentication.CreateAuthContext(connectData);

int endpointPort = 51300; // use your resolved or constrained DCOM endpoint.
await using DcomCallChannel channel = await DcomCallChannelFactory.ConnectTcpAsync(
    url.Host,
    endpointPort,
    authContext,
    cancellationToken).ConfigureAwait(false);
```

For Active Directory, use Kerberos with the same protection level.

```csharp
using System.Net;
using Opc.Classic;

OpcConnectData connectData = OpcConnectData.WithKerberos(
    url,
    new NetworkCredential("opc-reader@CORP.EXAMPLE.COM", password, "CORP.EXAMPLE.COM"),
    protectionLevel: OpcProtectionLevel.Privacy,
    operationTimeout: TimeSpan.FromSeconds(30));
```

## DCOM server listener: require privacy before production exposure

The current managed DA/AE/HDA host options expose `ListenAddress`, CLSID, ProgID, and friendly name. They do not expose a listener-level `OpcProtectionLevel` option yet, and the current managed TCP listener rejects authenticated PDUs. Keep these listeners on loopback or disposable interop networks unless an authenticated gateway or production DCOM host enforces packet privacy.

```csharp
using Microsoft.Extensions.DependencyInjection;
using Opc.Classic.Da.Hosting;
using Opc.Classic.Hosting;

builder.Services.AddClassicServer();
builder.Services.AddOpcDaServer<MyDaServer>(options =>
{
    options.Clsid = Guid.Parse("7f41b3e9-32ec-40c9-9e42-3e0e0fce5a11");
    options.ProgId = "Contoso.ManagedOpcDa.1";
    options.FriendlyName = "Contoso Managed OPC DA Server";
    options.ListenAddress = "127.0.0.1:51300";
});
```

When listener authentication policy is added, set its minimum protection to `OpcProtectionLevel.Privacy` rather than relying on clients to choose privacy.

## XML-DA client: use HTTPS and WS-Security policy

`HttpXmlDaClient` uses the `HttpClient` you supply. Use `https://`, validate certificates, and configure endpoint credentials on that `HttpClient`. If the XML-DA server requires WS-Security UsernameToken, signatures, or encryption, add those SOAP security headers in the endpoint-specific HTTP/SOAP layer before requests leave the process.

```csharp
using System.Net.Http.Headers;
using Opc.Classic.Xml;

using var handler = new SocketsHttpHandler();
using var httpClient = new HttpClient(handler)
{
    Timeout = TimeSpan.FromSeconds(30),
};
httpClient.DefaultRequestHeaders.Authorization =
    new AuthenticationHeaderValue("Bearer", accessToken);

var endpoint = new Uri("https://opcxml01.corp.example.com/OpcXmlDa");
var client = new HttpXmlDaClient(httpClient, endpoint);
```

Do not use plain `http://` for sensitive tag values outside isolated loopback tests.

## SMB named-pipe transport

The SMB2 client advertises signing and verifies signed responses when signing is negotiated and the caller supplies the NTLM/Kerberos SessionKey to `SessionSetupAsync`. SMB3 encryption is not available yet, and `ncacn_np` is not the default RPC transport.

```csharp
using Opc.Classic.Dcom.Smb;

await using var tcp = await TcpSmb2Transport.ConnectAsync("opc01.corp.example.com");
await using var connection = new Smb2Connection(
    new Smb2ConnectionOptions("opc01.corp.example.com"),
    tcp);

await connection.NegotiateAsync(cancellationToken);
await connection.SessionSetupAsync(
    serverBlob => ntlmProvider.Next(serverBlob),
    () => ntlmProvider.SessionKey,
    cancellationToken);
```

Require SMB signing on the server side for current deployments. Require SMB encryption only after SMB3 encryption support is available and validated against your Windows or Samba policy.

## Sample defaults

The repository DA/AE/HDA TCP sample clients use `NoOpAuthContext` and the managed server samples use the anonymous-only listener so the sample-container and loopback demos remain simple. Do not copy those defaults to production; build production connections with `OpcConnectData` and `OpcProtectionLevel.Privacy` where confidentiality is required.

See also [DCOM hardening and packet integrity](05-dcom-hardening-pkt-integrity-explainer.md), [Kerberos in Active Directory](03-kerberos-in-active-directory.md), [XML-DA client flows](06-xmlda-client-flows.md), and [Threat model](../security/THREAT_MODEL.md#sr-41-transport-confidentiality-posture).
