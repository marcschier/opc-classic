# escape=`

# opc-classic/testserver — OPC Foundation OpcTestServer_x64 reference server
#
# Multi-stage build:
#   1. build stage: windows/servercore:ltsc2022 + Visual Studio Build Tools 2022
#      (VCTools + ATL + CMake) builds the vendored CoreComponents CMake tree.
#   2. runtime stage: servercore + TestServer/TestClient/category manager +
#      the full proxy/stub DLL set + no-MSI DCOM registration on boot.
#
# OPERATOR: external/redist and external/redist/samples/OpcTestServer|OpcTestClient
# must be present in the Docker build context. If
# this vendor tree is intentionally omitted, build the artifacts out-of-band and
# restore external/redist/build/x64/Release via CI/cache before
# building.
# OPERATOR: the VS Build Tools install is slow (~30 min on a cold builder).
# Keep Docker layer caching and the GitHub Actions CoreComponents cache enabled.

FROM mcr.microsoft.com/windows/servercore:ltsc2022 AS build

SHELL ["powershell", "-NoProfile", "-Command", "$ErrorActionPreference = 'Stop'; $ProgressPreference = 'SilentlyContinue';"]

WORKDIR C:/src

COPY external/redist C:/src/external/redist
COPY external/redist/samples/OpcTestServer C:/src/external/redist/samples/OpcTestServer
COPY external/redist/samples/OpcTestClient C:/src/external/redist/samples/OpcTestClient
COPY external/tools/build-testserver.ps1 C:/src/external/tools/build-testserver.ps1

RUN $release = 'C:/src/external/redist/build/x64/Release'; `
    $required = @( `
        'OpcTestServer_x64.exe', `
        'OpcTestClient_x64.exe', `
        'OpcCategoryManager.exe', `
        'opccomn_ps.dll', `
        'opcproxy.dll', `
        'opc_aeps.dll', `
        'opcbc_ps.dll', `
        'OpcCmdPs.dll', `
        'OpcDxPs.dll', `
        'opchda_ps.dll', `
        'opcsec_ps.dll' `
    ); `
    $haveCachedArtifacts = (Test-Path -LiteralPath $release -PathType Container) -and (($required | Where-Object { -not (Test-Path -LiteralPath (Join-Path $release $_)) }).Count -eq 0); `
    if ($haveCachedArtifacts) { `
        Write-Host ('Using cached CoreComponents Release artifacts from {0}' -f $release); `
    } else { `
        Write-Host 'No complete cached CoreComponents Release output found; installing VS Build Tools 2022.'; `
        Invoke-WebRequest -Uri 'https://aka.ms/vs/17/release/vs_buildtools.exe' -OutFile C:/vs_buildtools.exe; `
        $vsArgs = @( `
            '--add', 'Microsoft.VisualStudio.Workload.VCTools', `
            '--add', 'Microsoft.VisualStudio.Component.VC.ATL', `
            '--add', 'Microsoft.VisualStudio.Component.VC.CMake.Project', `
            '--quiet', '--wait', '--norestart' `
        ); `
        $vs = Start-Process -FilePath C:/vs_buildtools.exe -ArgumentList $vsArgs -Wait -PassThru; `
        if ($vs.ExitCode -ne 0 -and $vs.ExitCode -ne 3010) { throw ('VS Build Tools install failed with exit code {0}' -f $vs.ExitCode) }; `
        Remove-Item C:/vs_buildtools.exe -Force; `
        C:/src/external/tools/build-testserver.ps1 -Configuration Release; `
    }; `
    New-Item -ItemType Directory -Path C:/out -Force | Out-Null; `
    foreach ($name in $required) { `
        $path = Join-Path $release $name; `
        if (-not (Test-Path -LiteralPath $path)) { throw ('CoreComponents artifact missing: {0}' -f $path) }; `
        Copy-Item -LiteralPath $path -Destination C:/out -Force; `
    }; `
    $configCandidates = @( `
        (Join-Path $release 'OpcTestServer_x64.config.xml'), `
        'C:/src/external/redist/samples/OpcTestServer/OpcTestServer.config.xml' `
    ); `
    $configSource = $configCandidates | Where-Object { Test-Path -LiteralPath $_ } | Select-Object -First 1; `
    if (-not $configSource) { throw 'OpcTestServer_x64.config.xml/OpcTestServer.config.xml not found in CoreComponents output or source tree.' }; `
    Copy-Item -LiteralPath $configSource -Destination C:/out/OpcTestServer_x64.config.xml -Force

FROM mcr.microsoft.com/windows/servercore:ltsc2022 AS runtime

SHELL ["powershell", "-NoProfile", "-Command", "$ErrorActionPreference = 'Stop'; $ProgressPreference = 'SilentlyContinue';"]

WORKDIR C:/server

LABEL maintainer="Opc.Classic .NET Contributors" `
      org.opencontainers.image.title="Opc.Classic OPC Foundation TestServer reference" `
      org.opencontainers.image.description="OPC Foundation OpcTestServer_x64, built from vendored CoreComponents, for Windows-container DCOM interop testing." `
      org.opencontainers.image.source="https://github.com/marcschier/opc-classic" `
      org.opencontainers.image.licenses="MIT"

COPY --from=build C:/out/ C:/server/
COPY external/docker/opc-testserver/dcom-test-acls.reg C:/dcom-test-acls.reg
COPY external/docker/opc-testserver/server-init.ps1 C:/server-init.ps1
COPY external/tools/register-testserver.ps1 C:/register-testserver.ps1

RUN reg import C:/dcom-test-acls.reg ; Remove-Item C:/dcom-test-acls.reg

EXPOSE 135/tcp
EXPOSE 49152-49200/tcp
EXPOSE 49152-49200/udp

ENTRYPOINT ["powershell", "-NoProfile", "-File", "C:/server-init.ps1"]
