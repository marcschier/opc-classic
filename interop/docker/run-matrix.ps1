# SPDX-License-Identifier: MIT
# Copyright (c) 2026 Opc.Classic .NET Contributors
#
# Build the Windows-container fleet and bring the server containers up
# for interactive interop testing (OpcTestClient_x64 against
# OpcTestServer_x64, OpcTestClient against managed-server, etc.).
#
# This used to be a conformance smoke matrix driven by the OPC
# Compliance Test Tool (CTT). The CTT integration was removed in
# 2026-06-10 because CTT v2.0.15 is GUI-only with no documented
# headless CLI -- no CI-friendly conformance verdict was possible.
# The remaining server containers are still useful for interactive
# interop runs and for the cross-impl-matrix python driver.
#
# Usage:
#   interop/docker/run-matrix.ps1                        # build + start
#   interop/docker/run-matrix.ps1 -SkipBuild             # use existing images
#   interop/docker/run-matrix.ps1 -IncludeTestServer     # also start OpcTestServer_x64 (requires external)
#   interop/docker/run-matrix.ps1 -SkipBuild -SkipUp     # just verify config

[CmdletBinding()]
param(
    [switch] $SkipBuild,
    [switch] $SkipUp,
    [switch] $IncludeTestServer
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
        $buildServices = @('c-server', 'managed-server', 'c-client')
        if ($IncludeTestServer) {
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

    if ($IncludeTestServer) {
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

# 2. Bring up the servers so interactive clients can attach.
if (-not $SkipUp) {
    Invoke-Step 'Starting server containers' {
        $serverServices = @('c-server', 'managed-server')
        if ($IncludeTestServer) {
            $serverServices += 'testserver'
        }
        docker compose --file $compose up -d @serverServices
    }

    Write-Host 'Waiting 15 seconds for DCOM registration to settle...' -ForegroundColor DarkGray
    Start-Sleep -Seconds 15

    Write-Host ''
    Write-Host '== Servers up ==' -ForegroundColor Green
    Write-Host '  c-server         (Opc.Classic.DaSample.1 via native C build)        opc-classic-c-server'
    Write-Host '  managed-server   (Opc.Classic.DaSample.1 via managed DA stack)      opc-classic-managed'
    if ($IncludeTestServer) {
        Write-Host '  testserver       (OpcTestServer_x64.1 via vendored CMake build)    opc-classic-testserver'
    }
    Write-Host ''
    Write-Host 'Run an interactive client against one of these hosts via:'
    Write-Host '  docker compose --file interop/docker/docker-compose.test.yml --profile interactive run --rm c-client'
    if ($IncludeTestServer) {
        Write-Host '  docker compose --file interop/docker/docker-compose.test.yml --profile interactive run --rm testclient'
    }
    Write-Host 'Tear down with:'
    Write-Host '  docker compose --file interop/docker/docker-compose.test.yml down'
}
