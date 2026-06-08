# escape=`

# opc-classic/testclient — OPC Foundation OpcTestClient_x64 reference client
#
# This image intentionally reuses the already-built opc-classic/testserver
# runtime image as its artifact source instead of running the slow
# CoreComponents CMake build a second time.
#
# OPERATOR: build opc-classic/testserver first. The compose/run-matrix scripts
# do this explicitly before building testclient.

FROM opc-classic/testserver AS testserver-artifacts

FROM mcr.microsoft.com/windows/servercore:ltsc2022 AS runtime

SHELL ["powershell", "-NoProfile", "-Command", "$ErrorActionPreference = 'Stop'; $ProgressPreference = 'SilentlyContinue';"]

WORKDIR C:/client

LABEL maintainer="Opc.Classic .NET Contributors" `
      org.opencontainers.image.title="Opc.Classic OPC Foundation TestClient reference" `
      org.opencontainers.image.description="OPC Foundation OpcTestClient_x64, copied from the TestServer build, for Windows-container DCOM interop smoke testing." `
      org.opencontainers.image.source="https://github.com/marcschier/opc-classic" `
      org.opencontainers.image.licenses="MIT"

COPY --from=testserver-artifacts C:/server/OpcTestClient_x64.exe C:/client/
COPY --from=testserver-artifacts C:/server/opccomn_ps.dll C:/client/
COPY --from=testserver-artifacts C:/server/opcproxy.dll C:/client/
COPY --from=testserver-artifacts C:/server/opc_aeps.dll C:/client/
COPY --from=testserver-artifacts C:/server/opcbc_ps.dll C:/client/
COPY --from=testserver-artifacts C:/server/OpcCmdPs.dll C:/client/
COPY --from=testserver-artifacts C:/server/OpcDxPs.dll C:/client/
COPY --from=testserver-artifacts C:/server/opchda_ps.dll C:/client/
COPY --from=testserver-artifacts C:/server/opcsec_ps.dll C:/client/
COPY external/docker/opc-testclient/dcom-test-acls.reg C:/dcom-test-acls.reg
COPY external/docker/opc-testclient/client.ps1 C:/client.ps1

RUN reg import C:/dcom-test-acls.reg ; Remove-Item C:/dcom-test-acls.reg

# Clients also expose 49152-49200 because DCOM callback objects can listen on
# the same ncacn_ip_tcp dynamic range.
EXPOSE 135/tcp
EXPOSE 49152-49200/tcp
EXPOSE 49152-49200/udp

ENTRYPOINT ["powershell", "-NoProfile", "-File", "C:/client.ps1"]
