# Cross-platform deployment for OPC Classic clients and servers

Opc.Classic is designed for .NET 10, NativeAOT-compatible libraries, and cross-platform operation. That does not make OPC Classic deployment magically simple. DCOM-era servers assume Windows naming, endpoint mapping, authentication levels, and service accounts. Linux and macOS clients bring container packaging, Kerberos files, DNS, time synchronization, and firewall rules into the picture. This tutorial turns the repository's architecture into a production deployment plan for clients and managed servers on Linux, macOS, containers, and Kubernetes.

For a compact Linux recipe see [../cookbook/01-connect-to-matrikon-from-linux.md](../cookbook/01-connect-to-matrikon-from-linux.md). For AOT details see [10-aot-and-trimming.md](10-aot-and-trimming.md). For authentication hardening see [04-security-with-kerberos-and-channel-binding.md](04-security-with-kerberos-and-channel-binding.md). For the repository sample Compose topology and exact environment variables, see [../../samples/README.docker.md](../../samples/README.docker.md).

## Prerequisites

- .NET 10 SDK for building and publishing.
- A tested OPC client or managed server application.
- Network access to any Windows DA/AE/HDA servers or to your managed server endpoint.
- For Kerberos: a KDC or Active Directory domain, DNS, NTP, SPNs, and keytab/password material.
- For Kubernetes: access to create Secrets, ConfigMaps, Deployments, Services, and probes.

## What you'll learn

- How to publish framework-dependent and NativeAOT binaries.
- How to build linux-x64 and linux-arm64 container images.
- How to carry `krb5.conf`, keytabs, and environment variables safely.
- How to define liveness/readiness checks for OPC Classic workloads.
- How to avoid common cross-platform DCOM and container pitfalls.

## Deployment decision table

| Workload | Recommended shape | Why |
| --- | --- | --- |
| Developer loopback client | `dotnet run` or framework-dependent publish | Fast iteration, easy logs. |
| Linux service connecting to Windows DA | Self-contained `linux-x64` or `linux-arm64` publish | No dependency on host runtime. |
| Small gateway container | NativeAOT publish in a distroless or chiseled image | Low startup time, small image, reduced attack surface. |
| Managed DA/AE/HDA server | Container or systemd service with stable endpoint | Server identity and firewall rules must be stable. |
| Kubernetes gateway | Deployment + Secret keytab + ConfigMap `krb5.conf` + probes | Repeatable rollout and credential rotation. |

## Publish modes

During early development use framework-dependent builds:

```bash
dotnet publish src/MyOpcClient/MyOpcClient.csproj -c Release -o artifacts/publish/MyOpcClient
```

For production Linux services, publish self-contained for the target runtime:

```bash
dotnet publish src/MyOpcClient/MyOpcClient.csproj \
  -c Release \
  -r linux-x64 \
  --self-contained true \
  -p:PublishSingleFile=true \
  -p:PublishTrimmed=true
```

For NativeAOT, start by validating the repository canary:

```powershell
dotnet publish samples\Opc.Classic.Samples.AotCanary -c Release -p:PublishAot=true -p:TreatWarningsAsErrors=true
```

Then publish your application:

```bash
dotnet publish src/MyOpcClient/MyOpcClient.csproj \
  -c Release \
  -r linux-x64 \
  -p:PublishAot=true \
  -p:TreatWarningsAsErrors=true
```

The contract is zero `IL2xxx` and `IL3xxx` warnings. If a warning appears, fix the root cause before deploying. Do not suppress trimming warnings casually; a suppressed warning can become a production-only failure when a generated proxy, codec, or configuration-bound type is removed.

## Multi-architecture container images

A practical Dockerfile for a NativeAOT client looks like this:

```dockerfile
# syntax=docker/dockerfile:1.7
ARG TARGETARCH
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY . .
RUN case "$TARGETARCH" in \
      amd64)  RID=linux-x64 ;; \
      arm64)  RID=linux-arm64 ;; \
      *) echo "unsupported arch $TARGETARCH" && exit 1 ;; \
    esac && \
    dotnet publish src/MyOpcClient/MyOpcClient.csproj \
      -c Release -r $RID -p:PublishAot=true -p:TreatWarningsAsErrors=true \
      -o /out

FROM mcr.microsoft.com/dotnet/runtime-deps:10.0-noble-chiseled
WORKDIR /app
COPY --from=build /out/ ./
USER $APP_UID
ENTRYPOINT ["./MyOpcClient"]
```

Build and push a multi-arch image:

```bash
docker buildx build \
  --platform linux/amd64,linux/arm64 \
  -t registry.example.com/opc/my-opc-client:1.0.0 \
  --push .
```

Use `runtime-deps` for NativeAOT because the .NET runtime is linked into the executable. Use `aspnet` or `runtime` images only when your app is framework-dependent. Keep image layers boring: no compilers, no shells unless your operational policy requires a debug image, and no keytab baked into the image.

## Runtime configuration

Use environment variables for non-secret settings and mounted files for secrets:

```bash
OPC__Url=opcda://opc01.plant.example.com/Matrikon.OPC.Simulation.1
OPC__AuthMode=Kerberos
OPC__ProtectionLevel=Integrity
KRB5_CONFIG=/etc/krb5.conf
KRB5_CLIENT_KTNAME=/var/run/secrets/opc/opc-client.keytab
```

If you use `Microsoft.Extensions.Configuration`, double underscores map to sections. Keep the configuration shape close to `OpcConnectData`: URL, authentication mode, protection level, timeout, realm, SPN, and credential source.

## Repository sample container convention

The repository samples use one TCP environment convention. Containerized DA/AE/HDA pairs exercise DCOM-over-IP between client/server containers. Server samples read `OPC_CLASSIC_SAMPLE_PORT` and default to DA `51300`, AE `51301`, HDA `51302`, CttServer `51303`, and OpcSecurityServer `51304`; `OPC_CLASSIC_LISTEN_ADDRESS` overrides the full bind address when you need more than `0.0.0.0:<port>`. Client samples read `OPC_CLASSIC_SERVER_HOST` and `OPC_CLASSIC_SERVER_PORT`; when both are present they call `DcomCallChannelFactory.ConnectTcpAsync` over `TcpClientTransport`, and when absent they fall back to the in-process `InMemoryCallChannel` path for local development.

```powershell
docker compose -f samples\docker-compose.yml up
```

The sample Compose file sets `OPC_CLASSIC_SERVER_HOST` to service DNS names (`daserver`, `aeserver`, `hdaserver`) and ports `51300`-`51302`. CttServer and OpcSecurityServer use the same server port convention when run directly, but they are not part of that multi-container client/server Compose topology. See [../../samples/README.docker.md](../../samples/README.docker.md) before copying sample port numbers into production; real Windows DCOM targets may still require endpoint mapper and constrained dynamic RPC ports.

## Kerberos on Linux

A minimal `/etc/krb5.conf` for an Active Directory realm:

```ini
[libdefaults]
  default_realm = PLANT.EXAMPLE.COM
  dns_lookup_realm = false
  dns_lookup_kdc = true
  rdns = false
  ticket_lifetime = 10h
  renew_lifetime = 7d
  forwardable = true

[realms]
  PLANT.EXAMPLE.COM = {
    kdc = dc01.plant.example.com
    kdc = dc02.plant.example.com
  }

[domain_realm]
  .plant.example.com = PLANT.EXAMPLE.COM
  plant.example.com = PLANT.EXAMPLE.COM
```

Important details:

- `rdns = false` avoids reverse-DNS surprises where the SPN is built from a PTR name instead of the name in your URL.
- KDC and client clocks must be synchronized. Five minutes of skew is enough to break Kerberos.
- DNS names must match the SPN you request, commonly `RPCSS/opc01.plant.example.com` for DCOM.
- Keytabs are secrets. Mount them with read-only permissions and rotate them through your secret-management process.

Validate outside the app first:

```bash
kinit -kt /var/run/secrets/opc/opc-client.keytab opc-client@PLANT.EXAMPLE.COM
kvno RPCSS/opc01.plant.example.com
klist -e
```

If `kvno` fails, the application will fail too. Fix realm, DNS, SPN, and key material before debugging Opc.Classic.

## Managed servers on Linux and macOS

Managed server hosting does not require the Windows COM runtime for the portable path, but clients still need to discover and activate the server. In production, choose a stable listen address and publish metadata through your deployment system:

```csharp
int port = int.TryParse(
    Environment.GetEnvironmentVariable("OPC_CLASSIC_SAMPLE_PORT"),
    out int parsedPort) && parsedPort > 0 ? parsedPort : 51300;
string listenAddress = Environment.GetEnvironmentVariable("OPC_CLASSIC_LISTEN_ADDRESS")
    ?? $"0.0.0.0:{port}";

builder.Services.AddOpcDaServer<MyServer>(options =>
{
    options.Clsid = Guid.Parse("4E3F63E7-4CC7-4E77-A59E-6462A1002001");
    options.ProgId = "Contoso.ManagedDa.1";
    options.FriendlyName = "Contoso Managed OPC DA Server";
    options.ListenAddress = listenAddress;
});
```

Do not use `127.0.0.1:0` outside development. Port zero is useful for tests because the OS chooses an available port; production clients and firewalls need a known endpoint. On macOS, treat the managed server as a development and gateway target unless you have a specific client discovery story. Most legacy DA clients that require COM registration are Windows-based.

## Kubernetes deployment

A Kubernetes client gateway typically needs a ConfigMap for `krb5.conf`, a Secret for the keytab, and probes that verify app-level readiness rather than just process existence.

```yaml
apiVersion: v1
kind: ConfigMap
metadata:
  name: opc-krb5
  namespace: plant-gateways
data:
  krb5.conf: |
    [libdefaults]
      default_realm = PLANT.EXAMPLE.COM
      dns_lookup_kdc = true
      rdns = false
---
apiVersion: v1
kind: Secret
metadata:
  name: opc-client-keytab
  namespace: plant-gateways
type: Opaque
data:
  opc-client.keytab: BASE64_ENCODED_KEYTAB
---
apiVersion: apps/v1
kind: Deployment
metadata:
  name: opc-da-gateway
  namespace: plant-gateways
spec:
  replicas: 2
  selector:
    matchLabels:
      app: opc-da-gateway
  template:
    metadata:
      labels:
        app: opc-da-gateway
    spec:
      containers:
      - name: gateway
        image: registry.example.com/opc/my-opc-client:1.0.0
        env:
        - name: OPC__Url
          value: opcda://opc01.plant.example.com/Matrikon.OPC.Simulation.1
        - name: OPC__AuthMode
          value: Kerberos
        - name: KRB5_CONFIG
          value: /etc/krb5/krb5.conf
        - name: KRB5_CLIENT_KTNAME
          value: /var/run/secrets/opc/opc-client.keytab
        volumeMounts:
        - name: krb5
          mountPath: /etc/krb5
          readOnly: true
        - name: keytab
          mountPath: /var/run/secrets/opc
          readOnly: true
        readinessProbe:
          httpGet:
            path: /health/ready
            port: 8080
          periodSeconds: 10
        livenessProbe:
          httpGet:
            path: /health/live
            port: 8080
          periodSeconds: 30
      volumes:
      - name: krb5
        configMap:
          name: opc-krb5
      - name: keytab
        secret:
          secretName: opc-client-keytab
```

The health endpoints are application-level. A readiness check should fail if the app cannot acquire credentials or cannot complete a lightweight OPC status call. A liveness check should be more conservative; restarting during a transient plant-network blip can make an outage worse. Use circuit breakers and backoff in the app, not aggressive liveness restarts.

## Network and firewall notes

DCOM commonly uses TCP/135 for endpoint mapping plus dynamic ports for object endpoints. Managed servers can choose a stable listen address, which is easier to firewall. Windows servers may need their dynamic RPC port range constrained. Document all flows:

- client to endpoint mapper;
- client to actual object endpoint;
- callbacks from server to client when DA subscriptions or AE callbacks are used;
- KDC traffic (`88/tcp` and `88/udp`) for Kerberos;
- DNS and NTP.

Containers add one more trap: callbacks may advertise container-internal addresses that the server cannot reach. Prefer explicit hostnames and stable callback endpoints for subscription-heavy deployments.

## Observability

Every deployment should log:

- target `OpcUrl` without passwords;
- auth mode and protection level;
- server vendor/version from `GetStatusAsync`;
- negotiated update rates and group handles;
- per-item HRESULT distribution;
- Kerberos realm/SPN and keytab path, but never key material.

For tracing, wrap your application service methods in `ActivitySource` spans. The library uses `ILogger` and generated proxy seams; application-level spans are the best place to add plant, line, server, and tag metadata.

## Pitfalls

- NativeAOT binaries are RID-specific. A `linux-x64` binary will not run on ARM64.
- Do not bake keytabs into images.
- Do not use `latest` image tags for plant systems.
- Do not assume container DNS canonicalization matches Active Directory SPNs.
- Do not publish a managed server on an ephemeral port and expect Windows clients to discover it.
- Do not treat a successful TCP connection as OPC readiness. Complete an authenticated status call.

## Blue/green and rollback strategy

OPC gateways often sit between modern deployment systems and equipment that changes slowly. A Kubernetes rolling update can restart a pod in seconds, but a plant operator may still see the gateway as a stateful bridge. Use readiness probes to keep new pods out of service until they have acquired credentials, completed a status call, and recreated required subscriptions. If the application uses callbacks, readiness should also confirm the advertised callback address is reachable from the server network.

For blue/green deployments, run both versions against a read-only workload first. Compare server status, browse counts, per-item read results, and callback rates. Only after read behavior matches should you allow writes from the new version. If the gateway writes setpoints, design rollback so only one version can write at a time. Split-brain writers are more dangerous than downtime.

Keep previous container images and keytab versions available for rollback. A deployment that changes both the binary and Kerberos key material should be staged: roll the keytab, validate tickets, then roll the binary; or roll the binary with old credentials, validate, then rotate credentials. Changing both at once makes failures ambiguous.

## Host-level service deployment

Containers are not required. A Linux systemd unit is often appropriate for a single gateway VM close to the plant network. A minimal unit file:

```ini
[Unit]
Description=Contoso OPC DA Gateway
After=network-online.target
Wants=network-online.target

[Service]
User=opc
Group=opc
Environment=KRB5_CONFIG=/etc/krb5.conf
Environment=KRB5_CLIENT_KTNAME=/etc/opc/opc-client.keytab
Environment=OPC__Url=opcda://opc01.plant.example.com/Matrikon.OPC.Simulation.1
ExecStart=/opt/contoso-opc/MyOpcClient
Restart=on-failure
RestartSec=10
NoNewPrivileges=true
PrivateTmp=true
ProtectSystem=strict
ProtectHome=true
ReadWritePaths=/var/lib/contoso-opc /var/log/contoso-opc

[Install]
WantedBy=multi-user.target
```

Use OS permissions to protect keytabs, logs, and any local queue. `NoNewPrivileges`, `ProtectSystem`, and a dedicated service account reduce blast radius. If you need packet captures during commissioning, create a separate debug unit or temporary capability grant instead of running the service as root.

## Configuration drift controls

Production failures often come from drift: DNS aliases change, firewall rules are removed, SPNs move, or a cluster node has wrong time. Add periodic self-checks that do not overload the server. A status call every minute, Kerberos ticket expiry logging, and NTP offset monitoring can warn you before callbacks fail. Store expected SPN, realm, and URL in configuration and log them at startup.

For Kubernetes, pin node pools that can reach the plant network. Do not let the scheduler move a gateway to a subnet without firewall access. Use PodDisruptionBudgets for redundant gateways and maintenance windows for single-instance gateways. Treat network policy as part of the application, not an afterthought.

## Example commissioning checklist

Before a gateway is accepted, run a checklist from the same subnet and identity it will use in production. Resolve the OPC host name. Acquire a Kerberos ticket or verify NTLMv2 credentials. Complete an authenticated status call. Browse the expected branch. Read a known good item and a known bad item. Create a subscription, force a refresh, and confirm callback delivery. Restart the process and confirm it recreates state without manual cleanup. Rotate logs and confirm no secrets are present. Reboot the node and verify the service starts after networking and time synchronization.

For containers, repeat the checklist after rescheduling the pod. A pod that works on one node can fail on another because of network policy, DNS, clock skew, or missing mounted secrets. Record the node name in startup logs during commissioning so drift is visible. For multi-architecture images, run the checklist on both amd64 and arm64 hardware if both are supported.

## Documentation handoff

Every cross-platform deployment should leave behind a concise handoff document. Include the image tag or binary version, runtime identifier, configured OPC URL, expected SPN, mounted secret names, health-check behavior, firewall flows, and rollback command. Also include the exact commands used during commissioning. Future maintainers should not have to rediscover which port range was opened or which keytab principal was mounted.

For regulated environments, attach evidence: publish logs showing AOT warnings as errors, container digest, vulnerability scan result, `klist` validation output with secrets removed, and a successful status-call log. This evidence is often the difference between a repeatable rollout and a one-off integration that nobody wants to touch.

## Maintenance review questions

At each release review, ask the same maintenance questions. Did any public configuration keys change? Did the expected server identity, ProgID, CLSID, SPN, or item namespace change? Did timeout, retry, or batch-size defaults change? Did the release add a dependency that affects deployment, security, or diagnostics? Did the runbook and screenshots still match the product? These questions are simple, but they catch many integration regressions before a plant outage does.

Also schedule periodic drills. Run the tutorial scenario in a staging environment, rotate credentials, restart the server, force a reconnect, and confirm logs explain what happened. Tutorials are most valuable when they stay executable.

## Next steps

- Publish an AOT canary with [10-aot-and-trimming.md](10-aot-and-trimming.md).
- Harden Kerberos and channel binding with [04-security-with-kerberos-and-channel-binding.md](04-security-with-kerberos-and-channel-binding.md).
- Diagnose deployment failures with [09-troubleshooting-and-diagnostics.md](09-troubleshooting-and-diagnostics.md).
- Review [../ARCHITECTURE.md](../ARCHITECTURE.md) for the transport and hosting layers, and [../../samples/README.docker.md](../../samples/README.docker.md) for runnable container samples.

## References

- [MS-DCOM] and [MS-RPCE] for DCOM activation and packet protection.
- [MS-KILE] for Kerberos behavior in Windows domains.
- OPC DA 3.00, AE 1.10, and HDA 1.20 for subscription and callback expectations.
- Repository samples: `samples\Opc.Classic.Samples.AotCanary` and [../../samples/README.docker.md](../../samples/README.docker.md).




