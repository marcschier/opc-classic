# DCOM container networking

OPC Classic interoperability tests need DCOM RPC traffic to cross container
boundaries. This document explains why the default Docker networking modes
do not work and which Windows-specific network drivers DO.

## The problem

DCOM activation produces a `DUALSTRINGARRAY` (per `[MS-DCOM] §2.2.19`) that
tells the client where to reach the activated object. The server populates
this array with bindings using the protocol-sequence + endpoint preferences
the client supplied, including the server's hostname or IP.

When the server runs inside a NAT-mode Docker container, the bindings
contain the container's INTERNAL IP address (e.g. `172.16.x.y`). The client
on the host network cannot route packets to `172.16.x.y` directly — NAT
hides it. Even if the client also runs inside a container on the same NAT
bridge, the SCM-published binding is unreachable from peer containers.

This is the same issue documented in the
[DcomContainerSample's open TODOs](https://github.com/wazzzaatosh/DcomContainerSample):

> Pester tests return `access denied` for the DcomTestExe test that it runs.
> Not sure if this is a networking problem, or an installation problem.
> Maybe internal networking uses NAT features which don't work right for DCOM.
> I tried transparent networking and it fails as well (but maybe with a
> different error?)

## Windows Docker network drivers

| Driver | When to use | DCOM-friendly? |
| --- | --- | --- |
| `nat` (default) | Single-container Linux-style isolation | ❌ Breaks OXID resolution |
| `transparent` | Container gets a routable IP on the host's physical network | ⚠️ Works in some configurations; reported flaky in `DcomContainerSample` |
| `l2bridge` | Containers share L2 with the host but get distinct IPs via static assignment | ✅ Recommended for our fleet |
| `l2tunnel` | Cloud / virtual switch scenarios | Same as `l2bridge` with different L2 forwarding |

Reference: [Microsoft — Container networking on Windows](https://learn.microsoft.com/en-us/virtualization/windowscontainers/container-networking/architecture).

## Our choice: `l2bridge`

`opc-test-net` is created as `l2bridge` with a static subnet:

```pwsh
docker network create `
    --driver l2bridge `
    --subnet 10.0.1.0/24 `
    --gateway 10.0.1.1 `
    opc-test-net
```

Containers are assigned fixed IPs in `external\docker\docker-compose.test.yml` so the
DCOM bindings advertise stable, peer-routable addresses.

### Functional managed DCOM-over-IP sample path (Track E)

The sample DA, AE, HDA, CttServer, and OPC Security servers support direct
DCOM-over-IP listeners without Windows SCM endpoint mapping. Server samples bind
`OPC_CLASSIC_SAMPLE_PORT` on `0.0.0.0` (defaults DA=51300, AE=51301,
HDA=51302, CttServer=51303, Security=51304), and DA/AE/HDA client samples dial
`OPC_CLASSIC_SERVER_HOST` / `OPC_CLASSIC_SERVER_PORT` through
`DcomCallChannelFactory.ConnectTcpAsync` and `TcpClientTransport`. When those
environment variables are absent, clients keep their original in-process
`InMemoryCallChannel` fallback for local development.

This Track E path is separate from the Windows COM/OXID dynamic-port path below:
it uses a known TCP port on the managed listener instead of SCM activation plus
endpoint-mapper-discovered object bindings. See `samples\README.docker.md` for
the DA/AE/HDA compose topology and `samples\README.md` for the full sample port table.

### Trade-offs

| Pro | Con |
| --- | --- |
| Containers can reach each other directly | Containers share L2 with the host network; not a strong isolation boundary |
| DCOM OXID bindings work as published by the server | Requires the host's physical NIC to support L2 forwarding (most do) |
| No `EXPOSE -P` port translation required between containers | Containers are reachable from the host LAN by IP — appropriate only for an isolated test LAN |

## Pinning the DCOM dynamic port range

DCOM's `IOXIDResolver` allocates a port from the dynamic range (default
49152-65535 on Windows Vista+) at activation time. To make `EXPOSE` a
bounded range possible, we pin the range to 49152-49200 via the registry:

```
HKLM\SOFTWARE\Microsoft\Rpc\Internet\Ports = 49152-49200 (REG_MULTI_SZ)
HKLM\SOFTWARE\Microsoft\Rpc\Internet\PortsInternetAvailable = Y
HKLM\SOFTWARE\Microsoft\Rpc\Internet\UseInternetPorts = Y
```

This is applied by every container's `dcom-test-acls.reg`. The `EXPOSE`
directive in the Dockerfiles is thus:

```dockerfile
EXPOSE 135/tcp                # RPC endpoint mapper
EXPOSE 49152-49200/tcp        # Pinned DCOM dynamic range
EXPOSE 49152-49200/udp        # Same for UDP-bound endpoints (rare in DCOM)
```

49 ports is enough headroom for a multi-server test (each server uses 1
port; each callback object on a client uses 1 port).

## Authentication: anonymous-only

Per `[MS-DCOM]` §2.2.18.1.1 the `IRemoteSCMActivator` activation can require
authentication levels from `RPC_C_AUTHN_LEVEL_NONE` (0x01) through
`RPC_C_AUTHN_LEVEL_PKT_PRIVACY` (0x06). Production deployments use
NTLM/Kerberos authentication at level 5 or 6.

For the test fleet we relax to anonymous (level 1) via the
`LegacyAuthenticationLevel = 1` and `LegacyImpersonationLevel = 2`
registry entries plus `DefaultAccessPermission` / `DefaultLaunchPermission`
ACLs granting `Everyone` + `ANONYMOUS LOGON`.

**This is correct for a disposable sandboxed test rig. It would be a
catastrophic security regression on any production host.** The `.reg` files
are committed to the repository with explicit "TEST RIG ONLY" warnings.

## Alternative: gMSA for production-grade testing

The reference [DcomContainerSample's Path B](https://github.com/wazzzaatosh/DcomContainerSample)
uses Group-Managed Service Accounts (gMSA) for Kerberos-authenticated DCOM
between containers in an Active Directory domain. This is the path to take
when:

- The interop test needs to validate signing/encryption (which our anonymous
  path bypasses).
- The fleet runs against production-style DCOM servers that reject anonymous
  connections.

Setting up gMSA-enabled Windows containers requires:

1. A Windows AD domain controller (real or `windows-containers-AD` from
   [plooploops](https://github.com/plooploops/windows-containers-AD)).
2. `New-CredentialSpec` against the gMSA account → bind into each container.
3. Adjust the DCOM ACLs to grant the gMSA SID instead of `Everyone`.

gMSA is outside the anonymous test fleet but remains the natural follow-up
when the fleet needs production-style authenticated DCOM.

## Troubleshooting matrix

| Symptom | Likely cause | Where to look |
| --- | --- | --- |
| `0x80070005 (E_ACCESSDENIED)` from CoCreateInstance | DCOM ACLs not applied / wrong reg view | `reg query "HKLM\SOFTWARE\Microsoft\Ole" /v EnableDCOM` |
| `0x80004005 (E_FAIL)` on the resolved binding | OXID resolver returned an unreachable IP (NAT) | `docker network inspect opc-test-net` — verify driver is `l2bridge` |
| Client hangs after CoCreateInstance | EPMapper port 135 blocked | `Test-NetConnection -ComputerName <peer> -Port 135` |
| Random `0x800706BA` (RPC_S_SERVER_UNAVAILABLE) | Dynamic-port allocation outside the EXPOSE range | `reg query "HKLM\SOFTWARE\Microsoft\Rpc\Internet"` — verify Ports = 49152-49200 |

## See also

- [MS-DCOM] §2.2.19 — DUALSTRINGARRAY format
- [MS-RPCE] §3.1.1.5 — ncacn_ip_tcp endpoint allocation
- `external\docker\opc-managed\dcom-test-acls.reg` — the source of truth for our ACL relaxations
- `samples\README.docker.md` — functional managed DCOM-over-IP sample topology
- `src\Opc.Classic.Dcom\Transport\DcomCallChannelFactory.cs` — direct TCP client helper used by the samples
- [DcomContainerSample](https://github.com/wazzzaatosh/DcomContainerSample) — reference repo
- [windows-containers-AD](https://github.com/plooploops/windows-containers-AD) — gMSA setup for the production-style path
