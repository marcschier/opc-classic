# OPCEnum DCOM authentication

OPCEnum (`OPC.ServerList.1`, CLSID `{13486D51-4821-11D2-A494-3CB306C10000}`) is used by discovery and by ProgID-based connect flows. On hardened Windows hosts (KB5004442 and follow-ups), activation must use at least `RPC_C_AUTHN_LEVEL_PKT_INTEGRITY`. The client now raises OPCEnum activation to packet integrity by default, preserves packet privacy when requested, and exposes the probe/MCP `authLevel` option.

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

## Troubleshooting

- `rpc_s_access_denied (0x05)`: the identity lacks OPCEnum Launch/Activation or Access permission.
- `RPC fault status 0x000006F7`: the hardened SCM rejected activation before an OPCEnum OBJREF was returned; re-check auth level, credentials, and AppID permissions.
- `ABSTRACT_SYNTAX_NOT_SUPPORTED` on legacy `IActivation`: the host does not support the fallback activation interface; fix OPCEnum AppID permissions or use direct CLSID connect.
