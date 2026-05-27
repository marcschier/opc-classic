# escape=`

# opc-classic/c-server — native C-built OPC DA reference server
#
# Multi-stage build:
#   1. build stage: dotnet-framework SDK image (includes MSBuild + Windows SDK
#      + MSVC build tools) compiles the hand-rolled OPC DA smoke server.
#   2. runtime stage: servercore + the built binary + /RegServer on boot
#
# The build stage compiles a hand-rolled native OPC DA smoke server from
# docker/opc-c-server/build/opc-sample-server.cpp. The older OPC Foundation
# Batch sample conversion remains documented as a future full-sample route.

FROM mcr.microsoft.com/dotnet/framework/sdk:4.8.1-windowsservercore-ltsc2022 AS build

SHELL ["powershell", "-NoProfile", "-Command", "$ErrorActionPreference = 'Stop'; $ProgressPreference = 'SilentlyContinue';"]

WORKDIR C:/build

COPY External/Include C:/build/External/Include
COPY docker/opc-c-server/build C:/build/build

RUN New-Item -ItemType Directory -Path C:/out -Force | Out-Null; `
    msbuild C:/build/build/opc-sample-server.sln `
        /p:Configuration=Release `
        /p:Platform=x64 `
        /p:OutDir=C:/out/ `
        /m

FROM mcr.microsoft.com/windows/servercore:ltsc2022 AS runtime

SHELL ["powershell", "-NoProfile", "-Command", "$ErrorActionPreference = 'Stop'; $ProgressPreference = 'SilentlyContinue';"]

WORKDIR C:/server

LABEL maintainer="Opc.Classic .NET Contributors" `
      org.opencontainers.image.title="Opc.Classic native C OPC DA reference server" `
      org.opencontainers.image.description="C-built OPC DA smoke server, headless, for cross-platform interop testing against the managed implementation." `
      org.opencontainers.image.source="https://github.com/marcschier/opc-classic" `
      org.opencontainers.image.licenses="MIT"

COPY --from=build C:/out/ C:/server/
COPY docker/opc-c-server/dcom-test-acls.reg C:/dcom-test-acls.reg
COPY docker/opc-c-server/server-init.ps1 C:/server-init.ps1

RUN reg import C:/dcom-test-acls.reg ; Remove-Item C:/dcom-test-acls.reg

EXPOSE 135/tcp
EXPOSE 49152-49200/tcp
EXPOSE 49152-49200/udp

ENTRYPOINT ["powershell", "-NoProfile", "-File", "C:/server-init.ps1"]
