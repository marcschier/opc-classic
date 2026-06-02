# Per-call MCP probe results (DA, HDA, AE)

See docs/interop/probe-matrikon.json + probe-testserver.json.
- Matrikon OPC Simulation 1: CLSID F8582CF2-88FB-11D0-B850-00C0F0104305 (vendor MSI install).
- OPC Foundation TestServer x64: CLSID F8582CF9-88FB-11DA-A5ED-0060B0692061 (built via tools/build-testserver.ps1; no MSI).

## Headline numbers

- Matrikon: 19/95 tools OK; 76 FAIL.
- TestServer: 10/95 OK (session + disconnect-without-connect only); 85 FAIL; da.connect timeout-blocked at 15s (CO_E_SERVER_EXEC_FAILURE).

## Matrikon DA: per-tool outcome

Tool | Result | Notes
---|---|---
opcclassic.session.create | OK | session bootstrap
opcclassic.session.list | OK | enumerates active sessions
opcclassic.session.close | OK | tears down session at end
opcclassic.da.connect | OK | CLSID-direct activation via IActivation::RemoteActivation (opnum 0)
opcclassic.da.disconnect | OK | clean disconnect after activation
opcclassic.da.get_status | OK | IOPCServer::GetStatus opnum 6; state=Running, version=2.0.0, vendor=Matrikon Inc.
opcclassic.da.browse (root) | OK | 5 root entries (#MonitorACLFile, @ClientCount, Configured Aliases, Simulation Items)
opcclassic.da.browse (Random leaves) | OK | 17 leaves found
opcclassic.da.read_items_by_id | OK | DA 3.0 stateless IOPCItemIO::Read; Random.Int4=19072 hr=0x0
opcclassic.da.add_group | OK | returns serverGroupHandle=18971112
opcclassic.da.remove_group | OK | clean tear-down after add
opcclassic.da.get_error_string | OK | round-trips HRESULT to message text
opcclassic.cpx.get_type_system | OK | reports namespace/supported state
opcclassic.da.get_properties | FAIL | AlterContext rejected ABSTRACT_SYNTAX_NOT_SUPPORTED (IOPCItemProperties IID not in initial bind)
opcclassic.da.add_items | FAIL | AlterContext rejected (IOPCItemMgt IID not in initial bind)
opcclassic.da.read_sync | FAIL | AlterContext rejected (IOPCSyncIO IID not in initial bind)
opcclassic.da.write_sync | FAIL | AlterContext rejected (IOPCSyncIO IID not in initial bind)
opcclassic.da.subscribe | FAIL | AlterContext rejected (IOPCAsyncIO2 / IConnectionPoint IIDs not in initial bind)
opcclassic.da.poll_subscription | FAIL | depends on .subscribe success

## TestServer: per-tool outcome

opcclassic.session.create / list / close | OK | session lifecycle
opcclassic.da.connect | FAIL | tools/call timed out (15s) - DCOM SCM activation blocked by CO_E_SERVER_EXEC_FAILURE; root cause documented in docs/interop/testserver.md (needs MSI install of proxy/stub DLLs into System32)
all other tools | FAIL | downstream of connect failure: session not connected
opcclassic.*.disconnect | OK | returns "client was not connected" hr=0x1; no actual call to server

## HDA / AE / Batch / Commands / DX / XML-DA on Matrikon

Matrikon OPC Simulation Server only implements OPC DA. Probing the HDA/AE/Batch/Commands/DX connect tools requires server-specific CLSIDs that the simulation server does not register. The probe consequently:

- opcclassic.{hda,ae,batch,commands,dx}.connect -> FAIL with "Provide an OPC server ProgID, CLSID, or connectionString." (probe sent no spec-specific clsid).
- All read/write/browse tools downstream of those connects -> FAIL ("session is not connected").
- opcclassic.{hda,ae,batch,commands,dx}.disconnect -> OK (no-op success).
opcclassic.discovery.enumerate_servers | FAIL | IRemoteSCMActivator::RemoteCreateInstance returned rpc_s_access_denied (0x05). Discovery now sends supplied NTLMv2/Kerberos/Windows-SSO credentials at packet-integrity (or privacy) activation level; the account still needs DCOM Launch/Activation and Access rights on the OPCEnum / OPC.ServerList AppID.

## Root causes (3 distinct issues)

### Issue A: AlterContext PROVIDER_REJECTION on per-spec sub-IIDs (Matrikon, 5+ DA tools)

src/Opc.Classic.Dcom/Transport/DcomCallChannel.cs::AlterContextAsync sends a single-IID alter_context_req with the new interface IID (IOPCItemProperties / IOPCItemMgt / IOPCSyncIO / IOPCAsyncIO2). Matrikon (and most production OPC servers) reject because their RPC endpoint did not advertise those IIDs during bind_ack. The DCE bind PDU should declare ALL anticipated interface IIDs at initial bind time so the server preloads their stub marshalers; an after-the-fact alter_context for an arbitrary new IID is a corner case real servers often refuse.

Fix surface: extend DcomCallChannel to: (a) precompute the full IID set for the connected spec (DA: IOPCServer + IOPCCommon + IOPCBrowse + IOPCBrowseServerAddressSpace + IOPCItemProperties + IOPCItemMgt + IOPCSyncIO + IOPCSyncIO2 + IOPCAsyncIO2 + IOPCAsyncIO3 + IOPCGroupStateMgt + IOPCGroupStateMgt2 + IConnectionPoint + IConnectionPointContainer + IOPCItemIO), (b) emit all of them in the initial BindPdu.ContextList instead of one-by-one. Alternative: fall back to a fresh bind on a NEW socket per spec interface (one channel per sub-interface).

### Issue B: TestServer activation fails (CO_E_SERVER_EXEC_FAILURE)

This probe was captured before Track AD: locally built TestServer EXE activation failed with HRESULT 0x80080005 and DCOM event log 10010 ("server did not register with DCOM within the required timeout") because the ad-hoc registration path only wrote HKLM entries. `tools/register-testserver.ps1` now performs the no-MSI setup that DCOM SCM needs: copy `opccomn_ps.dll` and `opcproxy.dll` to `%SystemRoot%\System32`, register those copies with native `regsvr32.exe`, and run `OpcTestServer_x64.exe /regserver` from the System32 working directory. Re-run the probe after elevated registration to refresh this report.

### Issue C: OPCEnum activation fails (rpc_s_access_denied)

opcclassic.discovery.enumerate_servers and ProgID-based connect paths use IRemoteSCMActivator::RemoteCreateInstance. The activator path now reuses the same OPC connection credentials used for the target server and upgrades weak activation protection to RPC_C_AUTHN_LEVEL_PKT_INTEGRITY (or preserves PKT_PRIVACY). Operators must still grant the calling identity DCOM Launch/Activation and Access permissions on the OPCEnum / OPC.ServerList AppID (CLSID `{13486D51-4821-11D2-A494-3CB306C10000}`) in Component Services; otherwise Windows will continue to return rpc_s_access_denied (0x05).

Workaround: pass a CLSID directly to connect tools (for example `--da-clsid`) to bypass OPCEnum, or call discovery/ProgID connect with NTLMv2/Kerberos/Windows-SSO credentials that have OPCEnum Launch/Activation and Access permissions. The probe driver also accepts `--auth-level pkt_integrity` or `--auth-level pkt_privacy` and forwards it to MCP discovery/connect tools. If a hardened host returns `RPC fault status 0x000006F7` for IRemoteSCMActivator or rejects legacy IActivation with `ABSTRACT_SYNTAX_NOT_SUPPORTED`, re-check OPCEnum DCOM security/registration on the server and continue using direct CLSID until the host accepts OPCEnum activation.

## What works today (Matrikon DA)

Server-object level interfaces work end-to-end: connect, get_status, browse, read_items_by_id (DA 3.0 stateless), add_group + remove_group, get_error_string, disconnect, plus all session-management tools. This covers the wire-format/NDR/MInterfacePointer paths exercised by Track Y/Z/AA tests.

## What is blocked today

Group-object level interfaces (add_items, read_sync, write_sync, subscribe, get_properties) require AlterContext for new IIDs; Matrikon rejects with ABSTRACT_SYNTAX_NOT_SUPPORTED. Fix requires Issue A above.

TestServer end-to-end activation requires the upstream MSI (Issue B). All non-DA specs (HDA / AE / Batch / Commands / DX) require installing matching servers; Matrikon Simulation does not provide them.
