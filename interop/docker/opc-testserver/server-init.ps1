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
$opcEnumRoot = 'C:/opcenum'
$opcEnumExe = Join-Path $opcEnumRoot 'OpcEnum.exe'
$opcEnumProxyStub = Join-Path $opcEnumRoot 'opccomn_ps.dll'
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
    if (-not (Test-Path -LiteralPath $opcEnumExe)) {
        $missing += 'OpcEnum.exe (x86)'
    }
    if (-not (Test-Path -LiteralPath $opcEnumProxyStub)) {
        $missing += 'opccomn_ps.dll (x86)'
    }
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

function Register-And-StartOpcEnum {
    $regsvr32 = Join-Path $env:WINDIR 'SysWOW64\regsvr32.exe'
    if (-not (Test-Path -LiteralPath $regsvr32)) {
        throw "x86 regsvr32.exe not found: $regsvr32"
    }

    Write-Host '-- Registering x86 OPC Common proxy/stub for OpcEnum'
    $registration = Start-Process $regsvr32 `
        -ArgumentList '/s', "`"$opcEnumProxyStub`"" `
        -Wait -PassThru -NoNewWindow
    if ($registration.ExitCode -ne 0) {
        throw "x86 opccomn_ps.dll registration failed with exit code $($registration.ExitCode)"
    }

    $service = Get-Service -Name OpcEnum -ErrorAction SilentlyContinue
    if (-not $service) {
        Write-Host '-- Registering x86 OpcEnum Windows service'
        $registration = Start-Process $opcEnumExe `
            -ArgumentList '/Service' `
            -WorkingDirectory $opcEnumRoot `
            -Wait -PassThru -NoNewWindow
        if ($registration.ExitCode -ne 0) {
            throw "OpcEnum.exe /Service failed with exit code $($registration.ExitCode)"
        }
        $service = Get-Service -Name OpcEnum -ErrorAction Stop
    }

    if ($service.Status -ne 'Running') {
        Start-Service -Name OpcEnum -ErrorAction Stop
        $service.WaitForStatus('Running', [TimeSpan]::FromSeconds(30))
    }
    if ((Get-Service -Name OpcEnum -ErrorAction Stop).Status -ne 'Running') {
        throw 'OpcEnum service did not reach the Running state.'
    }
    Write-Host 'OpcEnum service is registered and running'
}

function Unregister-OpcEnum {
    $service = Get-Service -Name OpcEnum -ErrorAction SilentlyContinue
    if ($service -and $service.Status -ne 'Stopped') {
        try {
            Stop-Service -Name OpcEnum -Force -ErrorAction Stop
            $service.WaitForStatus('Stopped', [TimeSpan]::FromSeconds(30))
        }
        catch {
            Write-Warning "Could not stop OpcEnum service: $_"
        }
    }

    if (Test-Path -LiteralPath $opcEnumExe) {
        $unregistration = Start-Process $opcEnumExe `
            -ArgumentList '/UnregServer' `
            -WorkingDirectory $opcEnumRoot `
            -Wait -PassThru -NoNewWindow
        if ($unregistration.ExitCode -ne 0) {
            Write-Warning "OpcEnum.exe /UnregServer exited with $($unregistration.ExitCode)"
        }
    }

    $regsvr32 = Join-Path $env:WINDIR 'SysWOW64\regsvr32.exe'
    if ((Test-Path -LiteralPath $regsvr32) -and (Test-Path -LiteralPath $opcEnumProxyStub)) {
        $unregistration = Start-Process $regsvr32 `
            -ArgumentList '/u', '/s', "`"$opcEnumProxyStub`"" `
            -Wait -PassThru -NoNewWindow
        if ($unregistration.ExitCode -ne 0) {
            Write-Warning "x86 opccomn_ps.dll unregistration exited with $($unregistration.ExitCode)"
        }
    }
}

function Register-TestServer {
    Write-Host '-- Registering OpcTestServer_x64.1 via interop/tools/register-testserver.ps1'
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

$process = $null
$opcEnumCleanupRequired = $false
$testServerCleanupRequired = $false
try {
    $opcEnumCleanupRequired = $true
    Register-And-StartOpcEnum

    $testServerCleanupRequired = $true
    Register-TestServer

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

    if ($testServerCleanupRequired) {
        Unregister-TestServer
    }
    if ($opcEnumCleanupRequired) {
        Unregister-OpcEnum
    }
}
