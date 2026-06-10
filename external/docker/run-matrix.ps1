# SPDX-License-Identifier: MIT
# Copyright (c) 2026 Opc.Classic .NET Contributors
#
# Build all fleet images and run the CTT smoke matrix:
#   - CTT vs native (C-built) server: baseline reference run
#   - CTT vs managed server: the actual interop validation
#
# Usage:
#   external/docker/run-matrix.ps1                        # build + run both smokes
#   external/docker/run-matrix.ps1 -SkipBuild             # use existing images
#   external/docker/run-matrix.ps1 -OnlyManaged           # only the managed-server smoke
#   external/docker/run-matrix.ps1 -IncludeTestServer     # also smoke OpcTestServer_x64.1 (requires external/redist)

[CmdletBinding()]
param(
    [switch] $SkipBuild,
    [switch] $OnlyManaged,
    [switch] $OnlyNative,
    [switch] $IncludeTestServer
)

$ErrorActionPreference = 'Stop'
$root = Split-Path -Parent $PSCommandPath
$compose = "$root/docker-compose.test.yml"
$results = "$root/results"
$runTestServer = $IncludeTestServer -and -not $OnlyManaged

if (-not (Test-Path $results)) {
    New-Item -ItemType Directory -Path $results -Force | Out-Null
}

function Invoke-Step {
    param([string] $Description, [scriptblock] $Body)
    Write-Host "==> $Description" -ForegroundColor Cyan
    & $Body
    if ($LASTEXITCODE -ne 0) {
        throw "Step failed: $Description (exit $LASTEXITCODE)"
    }
}

# 0. Ensure the l2bridge network exists (idempotent).
$existing = docker network ls --format '{{.Name}}' | Where-Object { $_ -eq 'opc-test-net' }
if (-not $existing) {
    Invoke-Step 'Creating opc-test-net l2bridge network' {
        docker network create --driver l2bridge `
            --subnet 10.0.1.0/24 --gateway 10.0.1.1 `
            opc-test-net
    }
}

# 1. Build (unless skipped).
if (-not $SkipBuild) {
    Invoke-Step 'Building fleet images' {
        $buildServices = @('c-server', 'managed-server', 'ctt', 'c-client')
        if ($runTestServer) {
            $buildServices += 'testserver'
        }

        # mcr.microsoft.com transient pull failures (rate-limit / CDN edge
        # blocks) are common from windows-2022 GH Actions runners.
        # Retry up to 3 times with backoff; fall through to the original
        # exit code on persistent failure.
        $maxAttempts = 3
        for ($attempt = 1; $attempt -le $maxAttempts; $attempt++) {
            docker compose --file $compose --profile interactive build @buildServices
            if ($LASTEXITCODE -eq 0) { break }
            if ($attempt -lt $maxAttempts) {
                $delay = 15 * $attempt
                Write-Host "==> docker compose build failed (exit $LASTEXITCODE); retrying in ${delay}s (attempt $($attempt + 1)/$maxAttempts)..." -ForegroundColor Yellow
                Start-Sleep -Seconds $delay
            }
        }
    }

    if ($runTestServer) {
        Invoke-Step 'Building OPC Foundation TestClient image from testserver artifacts' {
            $maxAttempts = 3
            for ($attempt = 1; $attempt -le $maxAttempts; $attempt++) {
                docker compose --file $compose --profile interactive build testclient
                if ($LASTEXITCODE -eq 0) { break }
                if ($attempt -lt $maxAttempts) {
                    $delay = 15 * $attempt
                    Write-Host "==> docker compose build testclient failed (exit $LASTEXITCODE); retrying in ${delay}s (attempt $($attempt + 1)/$maxAttempts)..." -ForegroundColor Yellow
                    Start-Sleep -Seconds $delay
                }
            }
        }
    }
}

# 2. Bring up the servers (always; CTT runs against them).
Invoke-Step 'Starting server containers' {
    $serverServices = @('c-server', 'managed-server')
    if ($runTestServer) {
        $serverServices += 'testserver'
    }

    docker compose --file $compose up -d @serverServices
}

try {
    # 3. Give the servers a moment to register their CLSIDs.
    Write-Host 'Waiting 15 seconds for DCOM registration to settle...' -ForegroundColor DarkGray
    Start-Sleep -Seconds 15

    # 4. Run the matrix.
    if (-not $OnlyManaged) {
        Invoke-Step 'CTT vs native (C-built) server [baseline]' {
            docker compose --file $compose run --rm ctt `
                -ProgId OPC.SampleServer.1 `
                -TargetHost opc-classic-c-server `
                -OutputPath C:/results/ctt-native.xml
        }
    }
    if ($runTestServer) {
        Invoke-Step 'CTT vs OPC Foundation TestServer [reference]' {
            docker compose --file $compose run --rm ctt `
                -ProgId OpcTestServer_x64.1 `
                -TargetHost opc-classic-testserver `
                -OutputPath C:/results/ctt-testserver.xml
        }
        Invoke-Step 'OpcTestClient vs OPC Foundation TestServer [reference]' {
            docker compose --file $compose run --rm testclient `
                -ProgId OpcTestServer_x64.1 `
                -TargetHost opc-classic-testserver
        }
    }
    if (-not $OnlyNative) {
        Invoke-Step 'CTT vs managed server [SUT]' {
            docker compose --file $compose run --rm ctt `
                -ProgId Opc.Classic.DaSample.1 `
                -TargetHost opc-classic-managed `
                -OutputPath C:/results/ctt-managed.xml
        }
    }
}
finally {
    Invoke-Step 'Tearing down server containers' {
        docker compose --file $compose down
    }
}

Write-Host ''
Write-Host '== Matrix complete ==' -ForegroundColor Green
Write-Host "Results: $results"
Get-ChildItem $results -Filter '*.xml' | ForEach-Object { Write-Host "  $($_.Name)  ($($_.Length) bytes)" }
