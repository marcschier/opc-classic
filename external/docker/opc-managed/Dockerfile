# escape=`

# opc-classic/managed — managed Opc.Classic.Samples.CttServer in a Windows
# container suitable for cross-container DCOM interop with the C-built
# reference server/client and the OPC Foundation TestClient.
#
# Note: a separate Dockerfile from the Linux variant at
# samples/Opc.Classic.Samples.CttServer/Dockerfile because:
#   1. We need windowsservercore base for DCOM ACL imports + reg work
#   2. We need to run --register --registry-hive=hklm before the hosted
#      service starts (Windows COM SCM activation path)
#   3. We need the DCOM port-pinning + relaxed-ACL .reg blob from this folder

FROM mcr.microsoft.com/dotnet/sdk:10.0-windowsservercore-ltsc2022 AS build
WORKDIR C:/src
COPY . .
RUN dotnet publish samples/Opc.Classic.Samples.CttServer/Opc.Classic.Samples.CttServer.csproj `
        -c Release `
        -r win-x64 `
        --self-contained false `
        -o C:/app/publish

FROM mcr.microsoft.com/dotnet/runtime:10.0-windowsservercore-ltsc2022 AS runtime

SHELL ["powershell", "-NoProfile", "-Command", "$ErrorActionPreference = 'Stop'; $ProgressPreference = 'SilentlyContinue';"]

WORKDIR C:/app

LABEL maintainer="Opc.Classic .NET Contributors" `
      org.opencontainers.image.title="Opc.Classic managed DA server (Windows fleet)" `
      org.opencontainers.image.description="Containerized managed OPC DA sample for the docker test fleet." `
      org.opencontainers.image.source="https://github.com/marcschier/opc-classic" `
      org.opencontainers.image.licenses="MIT"

COPY --from=build C:/app/publish ./
COPY external/docker/opc-managed/dcom-test-acls.reg C:/dcom-test-acls.reg
COPY external/docker/opc-managed/managed-init.ps1 C:/managed-init.ps1

# Apply relaxed DCOM ACLs (anonymous access; test rig only) + pin port range
RUN reg import C:/dcom-test-acls.reg ; Remove-Item C:/dcom-test-acls.reg

# Ports: 135 = RPC endpoint mapper; 49152-49200 = pinned DCOM dynamic range
EXPOSE 135/tcp
EXPOSE 49152-49200/tcp
EXPOSE 49152-49200/udp

ENTRYPOINT ["powershell", "-NoProfile", "-File", "C:/managed-init.ps1"]
