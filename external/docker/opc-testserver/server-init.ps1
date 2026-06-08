# SPDX-License-Identifier: MIT
# Copyright (c) 2026 Opc.Classic .NET Contributors
#
# ENTRYPOINT for opc-classic/testserver. Registers the OPC Foundation
# OpcTestServer_x64 no-MSI payload, starts the server, waits in the foreground,
# and unregisters on graceful shutdown.

[CmdletBinding()]
param(
    [switch] $Interactive
)

$ErrorActionPreference = 'Stop'

$serverRoot = 'C:/server'
$serverExe = Join-Path $serverRoot 'OpcTestServer_x64.exe'
$registerScript = 'C:/register-testserver.ps1'
$requiredArtifacts = @(
    'OpcTestServer_x64.exe',
    'OpcTestServer_x64.config.xml',
    'OpcTestClient_x64.exe',
    'OpcCategoryManager.exe',
    'opccomn_ps.dll',
    'opcproxy.dll',
    'opc_aeps.dll',
    'opcbc_ps.dll',
    'OpcCmdPs.dll',
    'OpcDxPs.dll',
    'opchda_ps.dll',
    'opcsec_ps.dll'
)

function Wait-Forever {
    while ($true) { Start-Sleep -Seconds 30 }
}

function Assert-ArtifactSet {
    $missing = $requiredArtifacts | Where-Object { -not (Test-Path -LiteralPath (Join-Path $serverRoot $_)) }
    if ($missing.Count -gt 0) {
        Write-Warning "Missing CoreComponents artifact(s): $($missing -join ', ')"
        if ($Interactive) {
            Write-Host 'Sleeping for `docker exec` access.'
            Wait-Forever
        }

        exit 1
    }

    if (-not (Test-Path -LiteralPath $registerScript)) {
        throw "Registration helper not found: $registerScript"
    }
}

function Start-OpcEnumIfPresent {
    $opcEnum = Get-Service -Name OpcEnum -ErrorAction SilentlyContinue
    if ($opcEnum -and $opcEnum.Status -ne 'Running') {
        Start-Service OpcEnum
        Write-Host 'Started OpcEnum service'
    } elseif (-not $opcEnum) {
        Write-Warning 'OpcEnum service is not present in this image. OPERATOR: verify whether CoreComponents should include/register OpcEnum for your test plan.'
    }
}

function Register-TestServer {
    Write-Host '-- Registering OpcTestServer_x64.1 via external/tools/register-testserver.ps1'
    & $registerScript -ExePath $serverExe
    if ($LASTEXITCODE -ne 0) {
        throw "register-testserver.ps1 failed with exit code $LASTEXITCODE"
    }
}

function Unregister-TestServer {
    if (-not (Test-Path -LiteralPath $registerScript)) {
        return
    }

    Write-Host '-- Unregistering OpcTestServer_x64.1'
    & $registerScript -ExePath $serverExe -Unregister
    if ($LASTEXITCODE -ne 0) {
        Write-Warning "register-testserver.ps1 -Unregister exited with $LASTEXITCODE"
    }
}

Write-Host '== opc-classic/testserver init =='
Assert-ArtifactSet
Start-OpcEnumIfPresent
Register-TestServer

$process = $null
try {
    Write-Host '-- Starting OPC Foundation TestServer (headless foreground wait)'
    $process = Start-Process -FilePath $serverExe -WorkingDirectory $serverRoot -PassThru
    Write-Host "OpcTestServer_x64.exe PID: $($process.Id)"

    while (-not $process.HasExited) {
        Start-Sleep -Seconds 5
        $process.Refresh()
    }

    Write-Host "OpcTestServer_x64.exe exited with code $($process.ExitCode)"
    exit $process.ExitCode
}
finally {
    if ($process -and -not $process.HasExited) {
        try {
            Stop-Process -Id $process.Id -Force -ErrorAction Stop
        }
        catch {
            Write-Warning "Could not stop OpcTestServer_x64.exe PID $($process.Id): $_"
        }
    }

    Unregister-TestServer
}
