# escape=`

# SPDX-License-Identifier: MIT
# Copyright (c) 2026 Opc.Classic .NET Contributors

FROM mcr.microsoft.com/dotnet/framework/sdk:4.8.1-20260414-windowsservercore-ltsc2022 AS build

SHELL ["powershell", "-NoProfile", "-Command", "$ErrorActionPreference = 'Stop'; $ProgressPreference = 'SilentlyContinue';"]
WORKDIR C:/build
COPY interop/inc C:/build/ext/inc
COPY interop/docker/opc-c-client/build C:/build/docker/opc-c-client/build
RUN msbuild C:/build/docker/opc-c-client/build/opc-test.sln /p:Configuration=Release /p:Platform=x64 /m; `
    New-Item -ItemType Directory -Path C:/out -Force | Out-Null; `
    Copy-Item C:/build/docker/opc-c-client/build/bin/x64/Release/opc-test.exe C:/out/opc-test.exe

FROM mcr.microsoft.com/windows/servercore:ltsc2022 AS runtime

SHELL ["powershell", "-NoProfile", "-Command", "$ErrorActionPreference = 'Stop'; $ProgressPreference = 'SilentlyContinue';"]
WORKDIR C:/client
LABEL maintainer="Opc.Classic .NET Contributors" `
      org.opencontainers.image.title="Opc.Classic native Matrikon proxy scaffold" `
      org.opencontainers.image.description="Windows-container native OPC DA client exerciser for authenticated DCOM smoke against the Linux managed SimulationServer." `
      org.opencontainers.image.licenses="MIT"
COPY --from=build C:/out/ C:/client/
COPY interop/docker/opc-c-client/dcom-test-acls.reg C:/dcom-test-acls.reg
COPY interop/docker/native-matrikon-proxy/run-native-proxy.ps1 C:/run-native-proxy.ps1
RUN reg import C:/dcom-test-acls.reg ; Remove-Item C:/dcom-test-acls.reg
EXPOSE 135/tcp
EXPOSE 49152-49200/tcp
EXPOSE 49152-49200/udp
ENTRYPOINT ["powershell", "-NoProfile", "-File", "C:/run-native-proxy.ps1"]
