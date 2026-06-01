#requires -Version 5.1
<#
.SYNOPSIS
    Register the OPC Foundation TestServer for DCOM activation.

.DESCRIPTION
    Writes the CLSID + LocalServer32 + DA 2.0 category entries to the
    machine-wide HKLM\SOFTWARE\Classes hive so Windows COM/DCOM SCM
    activation (CoCreateInstance, IActivation::RemoteActivation, etc.)
    can launch the TestServer.

    The DCOM Service Control Manager runs as SYSTEM and does NOT honor
    per-user HKCU registrations for LocalServer32 activation — only
    HKLM entries are visible. Hence this script requires admin
    elevation.

.PARAMETER ExePath
    Full path to OpcTestServer_x64.exe (default: looks for it under
    ext\CoreComponents\build\x64\Release\, the vendored CMake output
    produced by tools\build-testserver.ps1).

.PARAMETER Unregister
    Remove the HKLM entries instead of adding them.

.EXAMPLE
    .\tools\register-testserver.ps1                  # register
    .\tools\register-testserver.ps1 -Unregister      # remove
#>

[CmdletBinding()]
param(
    [string]$ExePath,
    [switch]$Unregister
)

$ErrorActionPreference = 'Stop'

$id = [System.Security.Principal.WindowsIdentity]::GetCurrent()
$principal = New-Object System.Security.Principal.WindowsPrincipal($id)
if (-not $principal.IsInRole([System.Security.Principal.WindowsBuiltInRole]::Administrator)) {
    Write-Error 'This script must be run from an elevated PowerShell window.'
    exit 1
}

$Clsid     = '{F8582CF9-88FB-11DA-A5ED-0060B0692061}'
$ProgId    = 'OpcTestServer_x64.1'
$ProgIdAny = 'OpcTestServer_x64'
$Catid20   = '{63D5F432-CFE4-11D1-B2C8-0060083BA1FB}'
$Catid10   = '{63D5F430-CFE4-11D1-B2C8-0060083BA1FB}'

if (-not $ExePath) {
    $repoRoot = Split-Path -Parent $PSScriptRoot
    $candidate = Join-Path $repoRoot 'ext\CoreComponents\build\x64\Release\OpcTestServer_x64.exe'
    if (Test-Path $candidate) { $ExePath = $candidate }
}
if (-not $ExePath -or -not (Test-Path $ExePath)) {
    Write-Error 'Cannot find OpcTestServer_x64.exe. Run tools\build-testserver.ps1 first, or pass -ExePath explicitly.'
    exit 1
}
$ExePath = (Resolve-Path $ExePath).Path

if ($Unregister) {
    Write-Host 'Unregistering TestServer from HKLM...'
    Remove-Item -Path "HKLM:\SOFTWARE\Classes\CLSID\$Clsid" -Recurse -Force -ErrorAction SilentlyContinue
    Remove-Item -Path "HKLM:\SOFTWARE\Classes\$ProgId" -Recurse -Force -ErrorAction SilentlyContinue
    Remove-Item -Path "HKLM:\SOFTWARE\Classes\$ProgIdAny" -Recurse -Force -ErrorAction SilentlyContinue
    Write-Host 'Done.'
    exit 0
}

Write-Host 'Registering TestServer to HKLM...'
Write-Host "  EXE:    $ExePath"
Write-Host "  CLSID:  $Clsid"
Write-Host "  ProgID: $ProgId"

& reg.exe add "HKLM\SOFTWARE\Classes\CLSID\$Clsid" /ve /t REG_SZ /d 'OPC DA 2.05a Test Server (x64)' /f *>$null
& reg.exe add "HKLM\SOFTWARE\Classes\CLSID\$Clsid\LocalServer32" /ve /t REG_SZ /d "`"$ExePath`"" /f *>$null
& reg.exe add "HKLM\SOFTWARE\Classes\CLSID\$Clsid\ProgID" /ve /t REG_SZ /d $ProgId /f *>$null
& reg.exe add "HKLM\SOFTWARE\Classes\CLSID\$Clsid\VersionIndependentProgID" /ve /t REG_SZ /d $ProgIdAny /f *>$null
& reg.exe add "HKLM\SOFTWARE\Classes\CLSID\$Clsid\Implemented Categories\$Catid20" /f *>$null
& reg.exe add "HKLM\SOFTWARE\Classes\CLSID\$Clsid\Implemented Categories\$Catid10" /f *>$null

& reg.exe add "HKLM\SOFTWARE\Classes\$ProgId" /ve /t REG_SZ /d 'OPC DA 2.05a Test Server (x64)' /f *>$null
& reg.exe add "HKLM\SOFTWARE\Classes\$ProgId\CLSID" /ve /t REG_SZ /d $Clsid /f *>$null
& reg.exe add "HKLM\SOFTWARE\Classes\$ProgIdAny" /ve /t REG_SZ /d 'OPC DA 2.05a Test Server (x64)' /f *>$null
& reg.exe add "HKLM\SOFTWARE\Classes\$ProgIdAny\CLSID" /ve /t REG_SZ /d $Clsid /f *>$null
& reg.exe add "HKLM\SOFTWARE\Classes\$ProgIdAny\CurVer" /ve /t REG_SZ /d $ProgId /f *>$null

Write-Host 'Done. Verify with:'
Write-Host "  python mcp/mcp_driver.py --testserver"
