# OPCEnum DCOM authentication

OPCEnum (`OPC.ServerList.1`, CLSID `{13486D51-4821-11D2-A494-3CB306C10000}`, AppID `{13486D44-4821-11D2-A494-3CB306C10000}`) is used by discovery and by ProgID-based connect flows. On hardened Windows hosts (KB5004442 and follow-ups), activation must use at least `RPC_C_AUTHN_LEVEL_PKT_INTEGRITY`. The client now raises OPCEnum activation to packet integrity by default, preserves packet privacy when requested, and exposes the probe/MCP `authLevel` option.

## Client options

Use credentials that have DCOM rights on the target OPCEnum AppID:

```powershell
python tools\probe_servers.py --da-progid Matrikon.OPC.Simulation.1 --auth-level pkt_integrity --request-timeout 30
```

For explicit credentials:

```powershell
python tools\probe_servers.py --da-progid Matrikon.OPC.Simulation.1 --username DOMAIN\user --password <password> --auth-level pkt_integrity
```

Use `pkt_privacy` when the environment requires encryption. Direct CLSID connect (for example `--da-clsid <clsid>`) bypasses OPCEnum and remains a workaround when OPCEnum activation is blocked.

## Server-side checklist

On the OPCEnum host:

1. Ensure the OPCEnum service is installed and running (`Get-Service OpcEnum`).
2. Open Component Services (`dcomcnfg`) > Computers > My Computer > DCOM Config > OPCEnum / OPC Server Enumerator.
3. On **Security**, customize **Launch and Activation Permissions** and grant the MCP/probe identity Local/Remote Launch and Local/Remote Activation as appropriate.
4. On **Security**, customize **Access Permissions** and grant the identity Local/Remote Access.
5. Confirm machine-level **COM Security** defaults do not deny the same identity.
6. Ensure Windows Firewall permits DCOM/RPC endpoint mapper TCP 135 and the configured dynamic RPC port range.
7. If using Windows SSO, run the MCP server/probe under the identity granted above.

Do not rely on disabling DCOM hardening. Configure AppID permissions and use packet integrity/privacy instead.

## Grant OPCEnum ACLs without `dcomcnfg`

`interop/tools/grant-opcenum-acl.ps1` automates the manual steps above by reading
the OPCEnum AppID's existing `AccessPermission` and `LaunchPermission`
REG_BINARY security descriptors, appending an
`(A;;CCDCLCSWRP;;;<SID>)` ACE for the supplied account, and writing the
merged descriptors back. Idempotent — re-running with the same `-Account`
is a no-op once the ACE is present. Requires elevated 64-bit PowerShell.

```powershell
# Grant the current user (default):
.\interop\tools\grant-opcenum-acl.ps1

# Grant a specific account:
.\interop\tools\grant-opcenum-acl.ps1 -Account "CORP\opcprobe"

# Grant the standard DCOM users group:
.\interop\tools\grant-opcenum-acl.ps1 -Account "BUILTIN\Distributed COM Users"

# Remove the ACE (rollback):
.\interop\tools\grant-opcenum-acl.ps1 -Unregister
```

### What the script does

For each of `HKLM:\SOFTWARE\Classes\AppID\{13486D44-...}` →
`AccessPermission` and `LaunchPermission`:

1. Reads the existing REG_BINARY security descriptor (or seeds a
   minimal default with `BUILTIN\Administrators`, `NT AUTHORITY\SYSTEM`,
   and `INTERACTIVE` granted if no descriptor exists yet).
2. Parses to SDDL form for human-readable manipulation.
3. Appends `(A;;CCDCLCSWRP;;;<SID>)` to the DACL where `CCDCLCSWRP` =
   `COM_RIGHTS_EXECUTE` + `COM_RIGHTS_EXECUTE_LOCAL` +
   `COM_RIGHTS_EXECUTE_REMOTE` + `COM_RIGHTS_ACTIVATE_LOCAL` +
   `COM_RIGHTS_ACTIVATE_REMOTE`. This matches what dcomcnfg writes when
   you tick all six boxes (Local/Remote Access + Local/Remote Launch +
   Local/Remote Activation).
4. Serializes back to binary and writes it under the same registry value.

Audit the resulting SDDL with:

```powershell
$reg = Get-Item 'HKLM:\SOFTWARE\Classes\AppID\{13486D44-4821-11D2-A494-3CB306C10000}'
$bytes = $reg.GetValue('AccessPermission')
$sd = New-Object System.Security.AccessControl.RawSecurityDescriptor($bytes, 0)
$sd.GetSddlForm('All')
```

## Troubleshooting

- `rpc_s_access_denied (0x05)`: the identity lacks OPCEnum Launch/Activation or Access permission.
- `RPC fault status 0x000006F7`: the hardened SCM rejected activation before an OPCEnum OBJREF was returned; re-check auth level, credentials, and AppID permissions.
- `ABSTRACT_SYNTAX_NOT_SUPPORTED` on legacy `IActivation`: the host does not support the fallback activation interface; fix OPCEnum AppID permissions or use direct CLSID connect.

## Related

- [`IOPCDataCallback` push delivery](da-callbacks.md) — the inbound-callback path has the same DCOM AppID / firewall / ACL prerequisites as OPCEnum, applied to the MCP host's listener.
- [Wire captures](wire-captures/README.md) — when authentication fails, enable the wire-capture diagnostic to inspect the actual bind PDU + fault payload.
- [Probe coverage](probe-coverage.md) — tool-by-tool status against Matrikon + TestServer, with OPCEnum-blocked tools called out.
