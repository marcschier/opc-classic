# escape=`

# opc-classic/c-client — native C-built OPC DA reference client (headless)
#
# Compiles the hand-rolled opc-test.exe (~190 lines) from
# docker/opc-c-client/build/opc-test.cpp via MSBuild/MSVC. The build/README.md
# documents the alternative path of wrapping the OPC Security 1.00 Sample
# Client's OPCTEST.cpp + OPCSPEED.cpp — kept as a future option for a richer
# throughput driver.
#
# Container ENTRYPOINT: client.ps1 invokes opc-test.exe with the ProgID +
# TargetHost forwarded from docker compose env / run args.

FROM mcr.microsoft.com/dotnet/framework/sdk:4.8.1-windowsservercore-ltsc2022 AS build

SHELL ["powershell", "-NoProfile", "-Command", "$ErrorActionPreference = 'Stop'; $ProgressPreference = 'SilentlyContinue';"]

WORKDIR C:/build

COPY External/Include C:/build/External/Include
COPY "External/OPC Security 1.00 Sample Code/Sample Client" "C:/build/sample-client"
COPY docker/opc-c-client/build C:/build/docker/opc-c-client/build

RUN msbuild C:/build/docker/opc-c-client/build/opc-test.sln `
        /p:Configuration=Release `
        /p:Platform=x64 `
        /m; `
    New-Item -ItemType Directory -Path C:/out -Force | Out-Null; `
    Copy-Item C:/build/docker/opc-c-client/build/bin/x64/Release/opc-test.exe C:/out/opc-test.exe

FROM mcr.microsoft.com/windows/servercore:ltsc2022 AS runtime

SHELL ["powershell", "-NoProfile", "-Command", "$ErrorActionPreference = 'Stop'; $ProgressPreference = 'SilentlyContinue';"]

WORKDIR C:/client

LABEL maintainer="Opc.Classic .NET Contributors" `
      org.opencontainers.image.title="Opc.Classic native C OPC DA reference client" `
      org.opencontainers.image.description="C-built headless OPC DA test client (OPCTEST + OPCSPEED) for cross-platform interop smoke against the managed server." `
      org.opencontainers.image.licenses="MIT (Opc.Classic shim) + OPC Foundation Sample Code license (sample sources)"

COPY --from=build C:/out/ C:/client/
COPY docker/opc-c-client/dcom-test-acls.reg C:/dcom-test-acls.reg
COPY docker/opc-c-client/client.ps1 C:/client.ps1

RUN reg import C:/dcom-test-acls.reg ; Remove-Item C:/dcom-test-acls.reg

# Clients also expose 49152-49200 because DCOM callback objects (IOPCDataCallback,
# IConnectionPoint subscriber proxies) listen on the SAME ncacn_ip_tcp ports.
EXPOSE 135/tcp
EXPOSE 49152-49200/tcp
EXPOSE 49152-49200/udp

ENTRYPOINT ["powershell", "-NoProfile", "-File", "C:/client.ps1"]
