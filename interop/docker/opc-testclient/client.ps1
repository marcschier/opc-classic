# SPDX-License-Identifier: MIT
# Copyright (c) 2026 Opc.Classic .NET Contributors
#
# ENTRYPOINT for opc-classic/testclient. Registers the CoreComponents
# proxy/stub DLLs, points local OpcEnum activation at the target TestServer
# container, runs OpcTestClient_x64.exe, and fails if the expected ProgID is not
# enumerated.

[CmdletBinding()]
param(
    [string] $ProgId = 'OpcTestServer_x64.1',
    [string] $TargetHost = 'opc-classic-testserver',
    [int] $StartupDelaySeconds = 5,
    [switch] $AllowMissingProgId,
    [switch] $Interactive
)

$ErrorActionPreference = 'Stop'

$clientRoot = 'C:/client'
$exe = Join-Path $clientRoot 'OpcTestClient_x64.exe'
$dllRegistrationOrder = @(
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
    $required = @('OpcTestClient_x64.exe') + $dllRegistrationOrder
    $missing = $required | Where-Object { -not (Test-Path -LiteralPath (Join-Path $clientRoot $_)) }
    if ($missing.Count -gt 0) {
        Write-Warning "Missing CoreComponents artifact(s): $($missing -join ', ')"
        if ($Interactive) {
            Write-Host 'Sleeping for `docker exec` access.'
            Wait-Forever
        }

        exit 1
    }
}

function Get-NativeSystemDirectory {
    $systemRoot = $env:SystemRoot
    if ([Environment]::Is64BitOperatingSystem -and -not [Environment]::Is64BitProcess) {
        $sysnative = Join-Path $systemRoot 'Sysnative'
        if (Test-Path -LiteralPath $sysnative) {
            return $sysnative
        }
    }

    return Join-Path $systemRoot 'System32'
}

function Invoke-CheckedNative {
    param(
        [Parameter(Mandatory = $true)]
        [string] $FilePath,

        [string[]] $ArgumentList = @(),

        [string] $WorkingDirectory,

        [Parameter(Mandatory = $true)]
        [string] $Description
    )

    Write-Host "  $Description"
    if ($WorkingDirectory) {
        Push-Location -LiteralPath $WorkingDirectory
    }

    try {
        & $FilePath @ArgumentList
        $exitCode = $LASTEXITCODE
    }
    finally {
        if ($WorkingDirectory) {
            Pop-Location
        }
    }

    if ($exitCode -ne 0) {
        throw "$Description failed with exit code $exitCode"
    }
}

function Register-ProxyStubs {
    $system32 = Get-NativeSystemDirectory
    $regsvr32 = Join-Path $system32 'regsvr32.exe'
    if (-not (Test-Path -LiteralPath $regsvr32)) {
        throw "Cannot find regsvr32.exe at $regsvr32"
    }

    foreach ($dll in $dllRegistrationOrder) {
        $source = Join-Path $clientRoot $dll
        $destination = Join-Path $system32 $dll
        Copy-Item -LiteralPath $source -Destination $destination -Force
        Invoke-CheckedNative -FilePath $regsvr32 -ArgumentList @('/s', $destination) -WorkingDirectory $system32 -Description "Registering $dll"
    }
}

function Unregister-ProxyStubs {
    $system32 = Get-NativeSystemDirectory
    $regsvr32 = Join-Path $system32 'regsvr32.exe'
    $reverseOrder = [object[]] $dllRegistrationOrder
    [array]::Reverse($reverseOrder)
    foreach ($dll in $reverseOrder) {
        $destination = Join-Path $system32 $dll
        if (-not (Test-Path -LiteralPath $destination)) {
            continue
        }

        try {
            Invoke-CheckedNative -FilePath $regsvr32 -ArgumentList @('/u', '/s', $destination) -WorkingDirectory $system32 -Description "Unregistering $dll"
        }
        catch {
            Write-Warning $_.Exception.Message
        }

        Remove-Item -LiteralPath $destination -Force -ErrorAction SilentlyContinue
    }
}

function Set-RemoteOpcEnumTarget {
    param([Parameter(Mandatory = $true)][string] $HostName)

    # OpcTestClient_x64.exe accepts no command-line target host. It calls
    # CoCreateInstance(CLSID_OpcServerList, ..., CLSCTX_ALL) and then enumerates
    # DA 2.0 categories. The AppID RemoteServerName value is the standard DCOM
    # way to redirect that local activation to a remote OpcEnum service.
    # OPERATOR: verify this redirection on the Windows Docker host; if the
    # upstream client later grows explicit remote-host flags, prefer those.
    $opcEnumClsid = '{13486D51-4821-11D2-A494-3CB306C10000}'
    $opcEnumAppId = '{13486D44-4821-11D2-A494-3CB306C10000}'
    $regExe = Join-Path (Get-NativeSystemDirectory) 'reg.exe'

    Invoke-CheckedNative -FilePath $regExe -ArgumentList @('add', "HKLM\SOFTWARE\Classes\CLSID\$opcEnumClsid", '/ve', '/t', 'REG_SZ', '/d', 'OPC Server List', '/f') -Description 'Writing OpcEnum CLSID default value'
    Invoke-CheckedNative -FilePath $regExe -ArgumentList @('add', "HKLM\SOFTWARE\Classes\CLSID\$opcEnumClsid", '/v', 'AppID', '/t', 'REG_SZ', '/d', $opcEnumAppId, '/f') -Description 'Writing OpcEnum CLSID AppID'
    Invoke-CheckedNative -FilePath $regExe -ArgumentList @('add', "HKLM\SOFTWARE\Classes\AppID\$opcEnumAppId", '/ve', '/t', 'REG_SZ', '/d', 'OPC Server List', '/f') -Description 'Writing OpcEnum AppID default value'
    Invoke-CheckedNative -FilePath $regExe -ArgumentList @('add', "HKLM\SOFTWARE\Classes\AppID\$opcEnumAppId", '/v', 'RemoteServerName', '/t', 'REG_SZ', '/d', $HostName, '/f') -Description "Pointing OpcEnum RemoteServerName at $HostName"
}

Write-Host '== opc-classic/testclient =='
Assert-ArtifactSet

if ($StartupDelaySeconds -gt 0) {
    Write-Host "Waiting $StartupDelaySeconds seconds for $TargetHost registration to settle..."
    Start-Sleep -Seconds $StartupDelaySeconds
}

$registeredProxyStubs = $false
try {
    Register-ProxyStubs
    $registeredProxyStubs = $true
    Set-RemoteOpcEnumTarget -HostName $TargetHost

    Write-Host "Invoking $exe against remote OpcEnum on $TargetHost; expecting ProgID $ProgId"
    $output = & $exe 2>&1
    $exitCode = $LASTEXITCODE
    $output | ForEach-Object { Write-Host $_ }

    if ($exitCode -ne 0) {
        exit $exitCode
    }

    $text = $output -join [Environment]::NewLine
    if ($ProgId -and -not $AllowMissingProgId -and $text -notmatch [regex]::Escape($ProgId)) {
        Write-Error "OpcTestClient_x64.exe exited successfully, but '$ProgId' was not present in the enumeration output."
        exit 1
    }

    exit 0
}
finally {
    if ($registeredProxyStubs) {
        Unregister-ProxyStubs
    }
}
