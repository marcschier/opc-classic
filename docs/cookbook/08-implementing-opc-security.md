# Implementing OPC Security

## What this covers

OPC Security 1.00 is an optional session-level identity switch layered above DCOM. The reference server in `samples\Opc.Classic.Samples.OpcSecurityServer` shows how to publish `IOPCSecurityNT` and `IOPCSecurityPrivate` beside a minimal DA server.

Run it with:

```powershell
dotnet run --project .\samples\Opc.Classic.Samples.OpcSecurityServer\Opc.Classic.Samples.OpcSecurityServer.csproj
```

The demo private credential is `operator` / `demo`. The implementation is intentionally a stub; do not copy its ACL behavior into production.

## How the sample is wired

`Program.cs` registers a minimal `IOpcDaServer`, a singleton `StubOpcSecurityServer`, and maps that singleton to:

- `IOpcSecurity` — the managed abstraction in `src\Opc.Classic.Security\IOpcSecurity.cs`.
- `IOPCSecurityNT` — the generated DCOM dispatcher contract for Windows-integrated authentication.
- `IOPCSecurityPrivate` — the generated DCOM dispatcher contract for server-private credentials.

The sample host publishes all DA root dispatchers plus `IOPCSecurityNTServerDispatcher` and `IOPCSecurityPrivateServerDispatcher` on the same managed TCP endpoint. That keeps the NoOpAuthContext demo path aligned with the other sample servers.

`StubOpcSecurityServer` implements the state machine:

- `SupportsWindowsAuthentication` and `SupportsPrivateAuthentication` return `true`.
- `LoginAsCurrentUserAsync` captures `WindowsIdentity.GetCurrent().Name` on Windows, or `OPC_CLASSIC_SAMPLE_IDENTITY` / `Environment.UserName` elsewhere.
- `LoginPrivateAsync` accepts only `operator` / `demo`.
- `LogoutAsync` clears `IsAuthenticated` and `CurrentIdentity`.
- The DCOM methods translate failures to `OpcException`; private re-logon uses `OpcSecurityErrors.OPC_E_PRIVATE_ACTIVE` from `src\Opc.Classic.Core\Errors\OpcSecurityErrors.cs`.

## DCOM-layer security vs OPC Security 1.00

| Layer | Purpose | Status in Opc.Classic |
| --- | --- | --- |
| DCOM/MSRPC authentication | Authenticates and protects the transport with NTLMv2 or Kerberos, packet integrity, and optional packet privacy. | Always part of the managed DCOM stack for real authenticated connections. Sample containers may use `NoOpAuthContext` only for disposable demos. |
| OPC Security 1.00 | Lets a connected client switch the server-side session identity with `ChangeUser`, `Logon`, and `Logoff`. | Optional. Servers publish these interfaces only when they have useful session-level identity policy. |

Use DCOM-layer authentication for connection trust and packet protection. Add OPC Security only when clients must change identity without creating a new COM object or transport connection.

## Replacing the stub in production

### Windows-account policy

Use the authenticated DCOM caller identity as the principal. In deployments that expose an application-level `OpcAuthMode.Windows` switch, back it with the current transport choices (`OpcAuthMode.NtlmV2` or `OpcAuthMode.Kerberos`) and validate roles before changing `CurrentIdentity`.

```csharp
if (policy.AuthMode == OpcAuthMode.Windows) // application policy over NTLMv2 or Kerberos transport
{
    using WindowsIdentity identity = WindowsIdentity.GetCurrent();
    if (!rolePolicy.IsOperator(identity.Name))
    {
        throw new OpcException(OpcResultId.Fail, "Caller is not authorized for OPC Security logon.");
    }

    currentIdentity = identity.Name;
}
```

On Windows COM hosts, the spec's NT credential approach calls `CoImpersonateClient`, queries the client blanket, and performs `AccessCheck` against the server ACL. Cross-platform hosts should use the identity established by the managed NTLMv2/Kerberos context and apply equivalent role checks.

### Private credential policy

Keep private OPC credentials in a server-owned store, never as plain text. Hash passwords with a modern password hashing scheme, compare in constant time where possible, and return an OPC failure HRESULT without revealing which field was wrong.

```csharp
CredentialRecord record = await credentialStore.FindByUserNameAsync(userId, cancellationToken);
if (!passwordHasher.Verify(record.PasswordHash, password))
{
    throw new OpcException(OpcResultId.Fail, "OPC Security private authentication failed.");
}

currentIdentity = "private:" + userId;
```

Use `OpcSecurityErrors.OPC_E_PRIVATE_ACTIVE` when a second private logon is attempted while private credentials are already active. Use a generic failure for invalid credentials.

## References

- `src\Opc.Classic.Security\IOpcSecurity.cs` — managed async-first OPC Security abstraction.
- `src\Opc.Classic.Security\Dcom\IOPCSecurityInterfaces.cs` — DCOM interface declarations and generated dispatcher inputs.
- `src\Opc.Classic.Core\Errors\OpcSecurityErrors.cs` — OPC Security HRESULT constants.
- `samples\Opc.Classic.Samples.OpcSecurityServer` — runnable reference server.
