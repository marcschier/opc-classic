# Security with Kerberos and channel binding

OPC Classic security is mostly DCOM security. Modern deployments must account for Microsoft DCOM hardening, NTLM relay risk, Kerberos service-principal identity, SPNEGO negotiation, channel binding, and operational realities such as keytab rotation. This tutorial shows how to move a production Opc.Classic client or managed server from the safe NTLMv2 baseline to Kerberos/SPNEGO with channel binding.

Opc.Classic provides self-contained NTLMv2, Kerberos, SPNEGO, and channel-binding token support through `OpcConnectData.WithKerberos`, `KerberosAuthInfo`, `KerberosAuthContext`, `ChannelBindings`, `ChannelBindingsFactory`, and `ChannelBindingsHash`. Managed listeners can require configured NTLMv2 authenticated binds; server-side Kerberos/SPNEGO acceptor wiring remains the listener-auth gap. Validate the exact package version, realm configuration, and protection level before enabling Kerberos for production traffic. The deployment and troubleshooting guidance applies because SPN, KDC, and channel-binding errors are independent of application code. For a compact recipe see [../cookbook/03-kerberos-in-active-directory.md](../cookbook/03-kerberos-in-active-directory.md); for NTLMv2 audit context see [../security/NTLMSSP_AUDIT_GUIDE.md](../security/NTLMSSP_AUDIT_GUIDE.md).

## Prerequisites

- Active Directory or another Kerberos realm reachable from client and server.
- DNS names that match the service principal you plan to request.
- Time synchronization on client, server, and KDC.
- A service account for the OPC endpoint and a user or service identity for the client.
- .NET 10 application using `Opc.Classic.Dcom.Kerberos` when Kerberos is enabled.

## What you'll learn

- How to choose SPNs for OPC/DCOM workloads.
- How to create and mount keytabs on Linux.
- How to construct `KerberosAuthInfo` and `KerberosAuthContext`.
- How channel binding tokens are computed.
- How to migrate from NTLMv2 to Kerberos safely.
- How to troubleshoot `KRB_AP_ERR_MODIFIED`, expired tickets, and CBT mismatches.

## Security baseline: NTLMv2 with packet integrity

The default Opc.Classic connection model is NTLMv2 plus `OpcProtectionLevel.Integrity`. That aligns with Microsoft's DCOM hardening (KB5004442), which rejects low authentication levels against patched Windows DCOM servers. NTLMv2 is acceptable for some isolated brownfield networks, but it has two drawbacks: the server identity is weaker than Kerberos service-principal identity, and NTLM relay is harder to reason about across proxies and gateways.

The baseline configuration looks like this:

```csharp
using System.Net;
using Opc.Classic;

OpcUrl url = OpcUrl.Parse("opcda://opc01.plant.example.com/Matrikon.OPC.Simulation.1");
var credentials = new NetworkCredential("opc-reader", password, "PLANT");
OpcConnectData connectData = OpcConnectData.WithNtlmV2(
    url,
    credentials,
    OpcProtectionLevel.Integrity,
    operationTimeout: TimeSpan.FromSeconds(30));
```

Do not downgrade to `Connect`, `Call`, or `Packet` to make an old test server work. Fix the server policy or isolate that legacy target. `Integrity` signs packets; `Privacy` signs and encrypts.

## Choose the service principal

DCOM commonly authenticates to an RPCSS-style SPN:

```text
RPCSS/opc01.plant.example.com@PLANT.EXAMPLE.COM
```

Use the fully qualified DNS name the client will use in the OPC URL. Avoid short names unless your realm and DNS policy require them. If the server runs under a domain service account, register the SPN on that account. In Active Directory:

```powershell
setspn -S RPCSS/opc01.plant.example.com PLANT\svc-opc-da
setspn -L PLANT\svc-opc-da
```

Use `-S`, not `-A`, so duplicate SPNs are rejected. Duplicate SPNs are a common cause of `KRB_AP_ERR_MODIFIED`: the KDC encrypted the ticket for one account, but the service tried to decrypt it with another account's key.

## Create a keytab for Linux clients or servers

For a Linux service identity, generate a keytab from the domain account. In many environments an AD administrator runs `ktpass` or an equivalent tool. Example:

```powershell
ktpass /princ opc-client@PLANT.EXAMPLE.COM `
  /mapuser PLANT\svc-opc-client `
  /crypto AES256-SHA1 `
  /ptype KRB5_NT_PRINCIPAL `
  /pass * `
  /out opc-client.keytab
```

Store the keytab as a secret. On a Linux host:

```bash
install -o opc -g opc -m 0400 opc-client.keytab /etc/opc/opc-client.keytab
export KRB5_CLIENT_KTNAME=/etc/opc/opc-client.keytab
```

Validate before running the app:

```bash
kinit -kt /etc/opc/opc-client.keytab opc-client@PLANT.EXAMPLE.COM
kvno RPCSS/opc01.plant.example.com
klist -e
```

If `kvno` cannot obtain a service ticket, the app cannot either.

## Configure krb5.conf

Use explicit realm configuration when containers or split DNS are involved:

```ini
[libdefaults]
  default_realm = PLANT.EXAMPLE.COM
  dns_lookup_realm = false
  dns_lookup_kdc = true
  rdns = false
  ticket_lifetime = 10h
  renew_lifetime = 7d

[realms]
  PLANT.EXAMPLE.COM = {
    kdc = dc01.plant.example.com
    kdc = dc02.plant.example.com
  }

[domain_realm]
  .plant.example.com = PLANT.EXAMPLE.COM
  plant.example.com = PLANT.EXAMPLE.COM
```

Set `KRB5_CONFIG=/etc/krb5.conf` in containers when you mount the file somewhere non-standard. Keep `rdns=false` unless your SPN strategy intentionally depends on reverse DNS.

## Construct Kerberos authentication in Opc.Classic

`KerberosAuthInfo` captures the realm, service principal, username, optional NetBIOS domain, password, and keytab path. Password flow is convenient for development; keytab flow is preferred for services.

```csharp
using Opc.Classic;
using Opc.Classic.Dcom.Kerberos;
using Opc.Classic.Security;

var kerberos = new KerberosAuthInfo(
    realm: "PLANT.EXAMPLE.COM",
    spn: "RPCSS/opc01.plant.example.com",
    username: "opc-client@PLANT.EXAMPLE.COM",
    domain: "PLANT",
    password: null,
    keytabPath: "/etc/opc/opc-client.keytab");

ChannelBindings? channelBindings = null;
var authContext = new KerberosAuthContext(
    kerberos,
    channelBindings,
    OpcProtectionLevel.Integrity);
```

The `KerberosAuthContext` builds SPNEGO tokens around the Kerberos AP-REQ/AP-REP exchange. That behavior is grounded in [MS-KILE] for Kerberos and RFC 4178 for SPNEGO. When packet signing/sealing is enabled in your library version, it must match [MS-RPCE] authentication verifier rules.

## Channel binding and Extended Protection

Channel binding ties authentication to the outer secure channel. For TLS-protected endpoints, MS-CSSP defines `tls-server-end-point:` followed by the hash of the DER-encoded server certificate. Opc.Classic exposes helpers for this structure:

```csharp
using Opc.Classic.Security;

byte[] serverCertificateDer = await File.ReadAllBytesAsync("opc-server.cer", cancellationToken);
ChannelBindings bindings = ChannelBindingsFactory.ForTlsServerEndpoint(serverCertificateDer);
byte[] cbtHash = ChannelBindingsHash.Compute(bindings);
```

`ChannelBindingsHash.Compute` serializes the RFC 2744 channel-binding structure and returns the MD5 checksum required by NTLMv2 and Kerberos channel-binding extensions. MD5 here is not a general-purpose password hash; it is the protocol-specified checksum over a structured binding blob.

When channel binding is enabled, client and server must compute the same binding. Mismatches happen when TLS terminates at a proxy, when the wrong certificate is hashed, or when the server expects no CBT while the client sends one. Document which endpoint certificate is authoritative.

## Migration plan: NTLMv2 to Kerberos

1. **Inventory URLs and identities.** List every OPC URL, Windows host, service account, and client account.
2. **Normalize DNS.** Decide whether clients use FQDNs. Update URLs before SPN registration.
3. **Register SPNs.** Use `setspn -S` and search for duplicates.
4. **Validate tickets outside the app.** Use `kinit`, `kvno`, and `klist` from Linux; use `klist get` or Windows Event Viewer on Windows.
5. **Enable Kerberos in a staging app.** Keep NTLMv2 fallback disabled at first so failures are visible.
6. **Add channel binding where applicable.** Only after basic Kerberos is stable.
7. **Roll out with metrics.** Track auth failures by code, target SPN, and host.
8. **Disable NTLM fallback by policy.** Do this only after every production path has been validated.

In code, keep both settings available but make fallback explicit:

```csharp
OpcConnectData connectData = useKerberos
    ? OpcConnectData.WithKerberos(url, credentials, OpcProtectionLevel.Integrity)
    : OpcConnectData.WithNtlmV2(url, credentials, OpcProtectionLevel.Integrity);
```

A silent fallback can hide broken SPNs for months. If you temporarily allow fallback, log it as a security warning.

## Troubleshooting Kerberos errors

### KRB_AP_ERR_MODIFIED

Usually means the service ticket was encrypted for a different account than the server is using. Check:

- duplicate SPN: `setspn -Q RPCSS/opc01.plant.example.com`;
- server process identity;
- keytab generated before a password reset;
- wrong hostname or alias in the OPC URL.

Regenerate the keytab after service-account password changes.

### KRB_AP_ERR_TKT_EXPIRED

The ticket expired before use. Check client clock, KDC clock, ticket lifetime, and long-running service refresh behavior. In containers, confirm NTP on the node. Restarting a pod may refresh credentials, but it is not a real fix for clock skew.

### Clock skew / KRB_AP_ERR_SKEW

Kerberos is time-sensitive. Keep client, server, and KDC within the realm skew policy, usually five minutes. Use chrony or systemd-timesyncd on Linux nodes.

### Cannot find KDC

Check `krb5.conf`, DNS SRV records, firewall rules for TCP/UDP 88, and container DNS policy. Set explicit `kdc =` entries for isolated clusters.

### CBT mismatch

Verify the exact certificate bytes used by client and server. If TLS terminates at a gateway, the client sees the gateway certificate, not the backend certificate. Either bind to the gateway certificate intentionally or disable CBT for that path after risk review.

## Operational guardrails

- Store keytabs as secrets and mount read-only.
- Rotate service-account keys and redeploy keytabs together.
- Prefer AES keys; avoid RC4-era account settings.
- Use `OpcProtectionLevel.Integrity` at minimum; prefer `Privacy` where encryption is required.
- Log SPN and realm, never passwords or keytab bytes.
- Monitor AD event logs and application logs together.

## Server-side identity and delegation choices

Kerberos gives you a chance to be explicit about server identity. Decide whether the OPC endpoint runs as LocalSystem, a group managed service account, or a normal domain service account. LocalSystem commonly maps to the machine account, which means the SPN belongs on the computer object. A domain service account makes rotation and ownership clearer but requires service configuration discipline. Group managed service accounts are attractive on Windows because password rotation is automatic, but Linux clients still need to request the SPN exactly as registered.

Avoid unconstrained delegation. OPC gateways rarely need to impersonate users to arbitrary downstream services. If a gateway must access another Kerberos-protected service on behalf of the user, use constrained delegation and document the exact target SPNs. Most DA, AE, and HDA read scenarios work with a service identity and application-level authorization instead of user delegation.

## Key rotation runbook

Create a written runbook for key rotation before the first incident. A safe sequence is:

1. Register or verify the SPN on the service account.
2. Generate a new keytab with current encryption types.
3. Store the keytab as a new secret version.
4. Deploy the new secret to one staging instance.
5. Run `kinit -kt`, `kvno`, and an application `GetStatus` call.
6. Roll the secret to production instances gradually.
7. Remove old secret versions after the maximum ticket lifetime has passed.

If you reset the service account password without updating every keytab, clients may receive tickets encrypted with keys the service cannot decrypt, producing `KRB_AP_ERR_MODIFIED`. Treat keytabs like certificates: version them, rotate intentionally, and keep expiry/age visible.

## Audit evidence

Security reviews need evidence, not only configuration. Capture these facts in deployment logs or audit records:

- authentication mode (`Kerberos` or `NtlmV2`);
- protection level (`Integrity` or `Privacy`);
- realm and SPN;
- whether channel binding is enabled;
- certificate thumbprint or CBT hash when TLS endpoint binding is used;
- ticket encryption type from `klist -e` during validation;
- server vendor/version from an authenticated status call.

Do not log passwords, AP-REQ blobs, session keys, or keytab bytes. If packet captures are required, treat them as sensitive artifacts because they contain authentication material and process metadata.

## When NTLMv2 remains necessary

Some brownfield servers cannot use Kerberos because they are not domain-joined, have broken SPN support, or run in isolated workgroups. In those cases, keep NTLMv2 but reduce risk: require packet integrity, scope firewall rules tightly, avoid credential reuse across environments, monitor for unexpected targets, and plan a gateway or server upgrade path. Do not enable NTLMv1 unless a formally accepted legacy exception exists.

## Lab-to-production differences

A Kerberos lab often uses one domain controller, one service account, and one DNS name. Production may have multiple KDCs, load-balanced aliases, service account password rotation, firewall inspection, TLS termination, and cross-realm trusts. Validate the production path explicitly. A ticket for `RPCSS/opc01.lab.example.com` proves little about `RPCSS/opc01.plant.example.com` if the latter is a CNAME, a cluster alias, or mapped to a different service account.

Document whether clients may use aliases. If an alias is allowed, register an SPN for that exact alias or configure canonicalization intentionally. Do not depend on reverse DNS to guess the right SPN. In container platforms, DNS search suffixes can change the apparent name; use fully qualified names in OPC URLs and SPNs.

## Policy decisions to record

Security architecture has choices that should be written down. Record whether NTLM fallback is allowed, which accounts own SPNs, whether channel binding is mandatory, whether packet privacy is required for specific network zones, and how keytabs are rotated. If an exception exists for an old workgroup server, document compensating controls and an expiry date. Undocumented exceptions become permanent risk.

Also record how clients identify servers. A plant may have friendly aliases, short names, FQDNs, and load balancer names for the same machine. Kerberos cares about the service name in the ticket, so configuration drift in naming can become an outage. Make the canonical OPC URL and SPN part of the approved deployment record.

## Maintenance review questions

At each release review, ask the same maintenance questions. Did any public configuration keys change? Did the expected server identity, ProgID, CLSID, SPN, or item namespace change? Did timeout, retry, or batch-size defaults change? Did the release add a dependency that affects deployment, security, or diagnostics? Did the runbook and screenshots still match the product? These questions are simple, but they catch many integration regressions before a plant outage does.

Also schedule periodic drills. Run the tutorial scenario in a staging environment, rotate credentials, restart the server, force a reconnect, and confirm logs explain what happened. Tutorials are most valuable when they stay executable.

## Next steps

- Deploy Kerberos files in containers with [03-cross-platform-deployment.md](03-cross-platform-deployment.md).
- Use [09-troubleshooting-and-diagnostics.md](09-troubleshooting-and-diagnostics.md) for log and trace recipes.
- Review packet-integrity rationale in [../cookbook/05-dcom-hardening-pkt-integrity-explainer.md](../cookbook/05-dcom-hardening-pkt-integrity-explainer.md).

## References

- [MS-KILE] for Windows Kerberos behavior.
- [MS-CSSP] section on channel binding / Extended Protection for Authentication.
- [MS-DCOM] and [MS-RPCE] for DCOM authentication levels and packet protection.
- Repository audit prep: [../security/NTLMSSP_AUDIT_GUIDE.md](../security/NTLMSSP_AUDIT_GUIDE.md).
- RFC 4178 for SPNEGO and RFC 5056/RFC 2744 for channel bindings.



