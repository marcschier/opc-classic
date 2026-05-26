# SPDX-License-Identifier: MIT
# Copyright (c) 2026 Opc.Classic .NET Contributors
#
# Build all fleet images and run the CTT smoke matrix:
#   - CTT vs native (C-built) server: baseline reference run
#   - CTT vs managed server: the actual interop validation
#
# Usage:
#   docker/run-matrix.ps1                        # build + run both smokes
#   docker/run-matrix.ps1 -SkipBuild             # use existing images
#   docker/run-matrix.ps1 -OnlyManaged           # only the managed-server smoke

[CmdletBinding()]
param(
    [switch] $SkipBuild,
    [switch] $OnlyManaged,
    [switch] $OnlyNative
)

$ErrorActionPreference = 'Stop'
$root = Split-Path -Parent $PSCommandPath
$compose = "$root/docker-compose.test.yml"
$results = "$root/results"

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
        docker compose --file $compose --profile interactive build
    }
}

# 2. Bring up the servers (always; CTT runs against them).
Invoke-Step 'Starting server containers' {
    docker compose --file $compose up -d c-server managed-server
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
