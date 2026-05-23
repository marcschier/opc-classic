# Kerberos handshake

This diagram shows the Kerberos path used when DCOM authentication is backed by Kerberos and SPNEGO. The client acquires a service ticket for the configured SPN, emits an AP-REQ in a GSS-API token, and requests mutual authentication so the server must answer with AP-REP.

`KerberosConnectionContext` owns the ticket acquisition and AP-REP processing state. `KerberosAuthContext` wraps that state behind `IAuthContext`, computes optional channel-binding hashes, wraps the AP-REQ in a SPNEGO initial token, and unwraps the server's response token from `NegTokenResp`.

The final packet-protection stage is shown as GSS-API token wrapping and MIC verification. The code already exposes the `IAuthContext.SignAndSeal` and `VerifyAndUnseal` seam; the Kerberos-specific `gss_get_mic`, `gss_wrap`, `gss_verify_mic`, and `gss_unwrap` implementation is marked as the follow-up work item in the current source.

```mermaid
sequenceDiagram
    autonumber
    participant App as Client app
    participant Krb as KerberosConnectionContext
    participant Kdc as KDC
    participant Spnego as SPNEGO wrapper
    participant Channel as DcomCallChannel
    participant Server as DCOM server

    App->>Krb: AcquireApRequestAsync(CBT hash)
    Krb->>Kdc: Authenticate client credential
    Kdc-->>Krb: TGT or credential context
    Krb->>Kdc: Request service ticket for SPN
    Kdc-->>Krb: Service ticket and AP-REQ state
    Krb-->>Spnego: GSS-API AP-REQ with mutual flag
    Spnego-->>Channel: NegTokenInit carrying AP-REQ
    Channel->>Server: RPC bind with AP-REQ token
    Server-->>Channel: bind_ack with NegTokenResp AP-REP
    Channel->>Spnego: Decode NegTokenResp
    Spnego->>Krb: Process AP-REP response token
    Krb-->>App: Derived session key
    App->>Channel: Invoke protected DCOM calls
    Channel->>Server: GSS MIC or wrap protected PDU
    Server-->>Channel: GSS MIC or wrapped response
```

## Where to read more

- [`src\Opc.Classic.Dcom.Kerberos\KerberosAuthContext.cs:13`](../../src/Opc.Classic.Dcom.Kerberos/KerberosAuthContext.cs#L13-L104) adapts Kerberos/SPNEGO to `IAuthContext` and documents the signing follow-up seam.
- [`src\Opc.Classic.Dcom.Kerberos\KerberosConnectionContext.cs:39`](../../src/Opc.Classic.Dcom.Kerberos/KerberosConnectionContext.cs#L39-L108) acquires AP-REQ tokens, requests mutual authentication, and processes AP-REP tokens.
- [`src\Opc.Classic.Dcom.Kerberos\IKerberosConnectionContext.cs:15`](../../src/Opc.Classic.Dcom.Kerberos/IKerberosConnectionContext.cs#L15-L37) defines the AP-REQ and AP-REP abstraction.
- [`src\Opc.Classic.Dcom.Kerberos\Spnego\SpnegoTokenBuilder.cs:13`](../../src/Opc.Classic.Dcom.Kerberos/Spnego/SpnegoTokenBuilder.cs#L13-L28) wraps Kerberos AP-REQ tokens in SPNEGO.
- Protocol references: [MS-KILE](https://learn.microsoft.com/openspecs/windows_protocols/ms-kile/) and [`docs\cookbook\03-kerberos-in-active-directory.md:36`](../cookbook/03-kerberos-in-active-directory.md#L36-L52).
