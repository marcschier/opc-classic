# escape=`

# opc-classic/c-server — native C-built OPC DA / Batch reference server
#
# Multi-stage build:
#   1. build stage: dotnet-framework SDK image (includes MSBuild + Windows SDK
#      + MSVC build tools) compiles the OPC Batch 2.0 Sample Server sources
#      from External/OPC Batch 2.00 Sample Code/Sample Server/
#   2. runtime stage: servercore + the built binaries + /RegServer on boot
#
# **PHASE-2 SCAFFOLD**: the build stage is currently a placeholder. The VS6
# .dsp files (External/OPC Batch 2.00 Sample Code/Sample Server/opc_exe.dsp and
# opc_dll.dsp) need to be converted to a modern .vcxproj before this image
# can be built. See docker/opc-c-server/build/README.md for the conversion
# checklist and current status.

FROM mcr.microsoft.com/dotnet/framework/sdk:4.8.1-windowsservercore-ltsc2022 AS build

SHELL ["powershell", "-NoProfile", "-Command", "$ErrorActionPreference = 'Stop'; $ProgressPreference = 'SilentlyContinue';"]

WORKDIR C:/build

COPY External/Include C:/build/External/Include
COPY "External/OPC Batch 2.00 Sample Code/Sample Server" "C:/build/sample-server"
COPY docker/opc-c-server/build C:/build/build

# TODO(docker-2-cserver): wire MSBuild to the hand-rolled vcxproj.
#
# The following invocation is the target; the build/opc-c-server.vcxproj
# referenced below is currently a placeholder/skeleton. See
# docker/opc-c-server/build/README.md.
#
# RUN msbuild C:/build/build/opc-c-server.sln `
#         /p:Configuration=Release `
#         /p:Platform=x64 `
#         /m

# Placeholder until the build is wired up: stub a fake EXE so the runtime
# stage can compose the rest of the layering.
RUN New-Item -ItemType Directory -Path C:/out -Force | Out-Null; `
    Set-Content -Path C:/out/opc_exe.exe -Value 'PLACEHOLDER' -Encoding ASCII

FROM mcr.microsoft.com/windows/servercore:ltsc2022 AS runtime

SHELL ["powershell", "-NoProfile", "-Command", "$ErrorActionPreference = 'Stop'; $ProgressPreference = 'SilentlyContinue';"]

WORKDIR C:/server

LABEL maintainer="Opc.Classic .NET Contributors" `
      org.opencontainers.image.title="Opc.Classic native C OPC DA reference server" `
      org.opencontainers.image.description="C-built OPC DA + Batch 2.0 sample server, headless, for cross-platform interop testing against the managed implementation." `
      org.opencontainers.image.source="https://github.com/marcschier/opc-classic" `
      org.opencontainers.image.licenses="MIT (Opc.Classic shim) + OPC Foundation Sample Code license (sample sources)"

COPY --from=build C:/out/ C:/server/
COPY docker/opc-c-server/dcom-test-acls.reg C:/dcom-test-acls.reg
COPY docker/opc-c-server/server-init.ps1 C:/server-init.ps1

RUN reg import C:/dcom-test-acls.reg ; Remove-Item C:/dcom-test-acls.reg

EXPOSE 135/tcp
EXPOSE 49152-49200/tcp
EXPOSE 49152-49200/udp

ENTRYPOINT ["powershell", "-NoProfile", "-File", "C:/server-init.ps1"]
