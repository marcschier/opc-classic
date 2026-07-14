# escape=`

# opc-classic/testserver — OPC Foundation OpcTestServer_x64 reference server
#
# The self-hosted workflow builds the vendored interop CMake tree on the host
# first, then this image consumes the verified x64 TestServer/TestClient
# artifacts plus the x86 OpcEnum service and OPC Common proxy/stub. Keeping
# MSVC/CMake outside the container avoids a second cold Visual Studio Build
# Tools install and keeps the Docker build tied only to the vendored interop
# tree and its generated Release output.
#
# OPERATOR: run both interop/tools/build-testserver.ps1 and
# interop/tools/build-opcenum.ps1 with -Configuration Release before invoking
# docker compose directly. The workflow performs clean builds as needed and
# fails before docker build if any required artifact is absent.

FROM mcr.microsoft.com/windows/servercore:ltsc2022 AS runtime

SHELL ["powershell", "-NoProfile", "-Command", "$ErrorActionPreference = 'Stop'; $ProgressPreference = 'SilentlyContinue';"]

WORKDIR C:/server

LABEL maintainer="Opc.Classic .NET Contributors" `
      org.opencontainers.image.title="Opc.Classic OPC Foundation TestServer reference" `
      org.opencontainers.image.description="OPC Foundation OpcTestServer_x64, built from vendored CoreComponents, for Windows-container DCOM interop testing." `
      org.opencontainers.image.source="https://github.com/marcschier/opc-classic" `
      org.opencontainers.image.licenses="MIT"

COPY interop/build/x64/Release/OpcTestServer_x64.exe C:/server/
COPY interop/build/x64/Release/OpcTestServer_x64.config.xml C:/server/
COPY interop/build/x64/Release/OpcTestClient_x64.exe C:/server/
COPY interop/build/x64/Release/OpcCategoryManager.exe C:/server/
COPY interop/build/x64/Release/opccomn_ps.dll C:/server/
COPY interop/build/x64/Release/opcproxy.dll C:/server/
COPY interop/build/x64/Release/opc_aeps.dll C:/server/
COPY interop/build/x64/Release/opcbc_ps.dll C:/server/
COPY interop/build/x64/Release/OpcCmdPs.dll C:/server/
COPY interop/build/x64/Release/OpcDxPs.dll C:/server/
COPY interop/build/x64/Release/opchda_ps.dll C:/server/
COPY interop/build/x64/Release/opcsec_ps.dll C:/server/
COPY interop/build/x86/Release/OpcEnum.exe C:/opcenum/
COPY interop/build/x86/Release/opccomn_ps.dll C:/opcenum/
COPY interop/docker/opc-testserver/dcom-test-acls.reg C:/dcom-test-acls.reg
COPY interop/docker/opc-testserver/server-init.ps1 C:/server-init.ps1
COPY interop/tools/register-testserver.ps1 C:/register-testserver.ps1

RUN reg import C:/dcom-test-acls.reg ; Remove-Item C:/dcom-test-acls.reg

EXPOSE 135/tcp
EXPOSE 49152-49200/tcp
EXPOSE 49152-49200/udp

ENTRYPOINT ["powershell", "-NoProfile", "-File", "C:/server-init.ps1"]
