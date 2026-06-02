#requires -Version 5.1
<#
.SYNOPSIS
    Register the OPC Foundation TestServer for DCOM activation.

.DESCRIPTION
    Installs the locally built x64 OPC DA/Common proxy-stub DLLs into
    the native Windows System32 directory, registers those System32
    copies with regsvr32, runs OpcTestServer_x64.exe /regserver with
    System32 as the working directory, and writes compatibility CLSID,
    ProgID, LocalServer32, and DA category entries under
    HKLM\SOFTWARE\Classes.

    The DCOM Service Control Manager runs as SYSTEM and does NOT honor
    per-user HKCU registrations for LocalServer32 activation — only
    HKLM entries are visible. Hence this script requires admin
    elevation from 64-bit PowerShell.

.PARAMETER ExePath
    Full path to OpcTestServer_x64.exe (default: looks for it under
    ext\CoreComponents\build\x64\Release\, the vendored CMake output
    produced by tools\build-testserver.ps1). The sibling
    opccomn_ps.dll and opcproxy.dll files are also required.

.PARAMETER Unregister
    Run OpcTestServer_x64.exe /unregserver when the EXE is available,
    remove the TestServer HKLM entries, unregister the copied
    proxy-stub DLLs from System32, and delete them if present.

.EXAMPLE
    .\tools\register-testserver.ps1                  # register
    .\tools\register-testserver.ps1 -Unregister      # remove TestServer entries and copied DLLs
#>

[CmdletBinding()]
param(
    [string]$ExePath,
    [switch]$Unregister
)

$ErrorActionPreference = 'Stop'

function Test-IsAdministrator {
    $id = [System.Security.Principal.WindowsIdentity]::GetCurrent()
    $principal = New-Object System.Security.Principal.WindowsPrincipal($id)
    return $principal.IsInRole([System.Security.Principal.WindowsBuiltInRole]::Administrator)
}

function Resolve-DefaultTestServerPath {
    $repoRoot = Split-Path -Parent $PSScriptRoot
    $candidate = Join-Path $repoRoot 'ext\CoreComponents\build\x64\Release\OpcTestServer_x64.exe'
    if (Test-Path -LiteralPath $candidate) {
        return $candidate
    }

    return $null
}

function Resolve-SystemRoot {
    $systemRoot = $env:SystemRoot
    if (-not $systemRoot) {
        $systemRoot = Split-Path -Parent ([Environment]::GetFolderPath([Environment+SpecialFolder]::System))
    }

    return $systemRoot
}

function Resolve-System32Directory {
    return Join-Path (Resolve-SystemRoot) 'System32'
}

function Resolve-NativeSystemDirectory {
    $systemRoot = Resolve-SystemRoot

    if ([Environment]::Is64BitOperatingSystem -and -not [Environment]::Is64BitProcess) {
        $sysnative = Join-Path $systemRoot 'Sysnative'
        if (Test-Path -LiteralPath $sysnative) {
            return $sysnative
        }
    }

    return Join-Path $systemRoot 'System32'
}

function Invoke-CheckedCommand {
    param(
        [Parameter(Mandatory = $true)]
        [string]$FilePath,

        [string[]]$ArgumentList = @(),

        [string]$WorkingDirectory,

        [Parameter(Mandatory = $true)]
        [string]$Description
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
        $displayArgs = if ($ArgumentList.Count -gt 0) { ' ' + ($ArgumentList -join ' ') } else { '' }
        throw "$Description failed (exit $exitCode): $FilePath$displayArgs"
    }
}

function Invoke-RegExe {
    param(
        [Parameter(Mandatory = $true)]
        [string[]]$ArgumentList
    )

    & $script:RegExePath @ArgumentList *>$null
    if ($LASTEXITCODE -ne 0) {
        throw "reg.exe $($ArgumentList -join ' ') failed (exit $LASTEXITCODE)."
    }
}

function Remove-RegKey {
    param(
        [Parameter(Mandatory = $true)]
        [string]$KeyPath
    )

    & $script:RegExePath delete $KeyPath /f *>$null
}

function Set-ProxyStubInstallMarker {
    param(
        [Parameter(Mandatory = $true)]
        [string]$DllName,

        [Parameter(Mandatory = $true)]
        [string]$DllPath,

        [Parameter(Mandatory = $true)]
        [string]$RegistrationPath
    )

    $hash = (Get-FileHash -LiteralPath $DllPath -Algorithm SHA256).Hash
    Invoke-RegExe -ArgumentList @('add', $script:InstallMarkerRegPath, '/v', "$DllName.Path", '/t', 'REG_SZ', '/d', $RegistrationPath, '/f')
    Invoke-RegExe -ArgumentList @('add', $script:InstallMarkerRegPath, '/v', "$DllName.Sha256", '/t', 'REG_SZ', '/d', $hash, '/f')
}

function Get-ProxyStubInstallMarkerHash {
    param(
        [Parameter(Mandatory = $true)]
        [string]$DllName
    )

    $property = Get-ItemProperty -Path $script:InstallMarkerProviderPath -Name "$DllName.Sha256" -ErrorAction SilentlyContinue
    if (-not $property) {
        return $null
    }

    return $property.PSObject.Properties["$DllName.Sha256"].Value
}

function Install-ProxyStubDlls {
    param(
        [Parameter(Mandatory = $true)]
        [string]$ArtifactDirectory,

        [Parameter(Mandatory = $true)]
        [string]$SystemDirectory,

        [Parameter(Mandatory = $true)]
        [string]$RegistrationDirectory
    )

    $dllNames = @('opccomn_ps.dll', 'opcproxy.dll')
    $installedDlls = @()

    foreach ($dllName in $dllNames) {
        $source = Join-Path $ArtifactDirectory $dllName
        if (-not (Test-Path -LiteralPath $source)) {
            throw "Cannot find $dllName beside $ExePath. Run tools\build-testserver.ps1 first, or pass -ExePath pointing at a complete build output directory."
        }

        $source = (Resolve-Path -LiteralPath $source).Path
        $destination = Join-Path $SystemDirectory $dllName
        $registrationPath = Join-Path $RegistrationDirectory $dllName
        Write-Host "  Copying $dllName to $RegistrationDirectory"
        Copy-Item -LiteralPath $source -Destination $destination -Force
        Set-ProxyStubInstallMarker -DllName $dllName -DllPath $destination -RegistrationPath $registrationPath
        $installedDlls += $registrationPath
    }

    foreach ($dllPath in $installedDlls) {
        $dllName = Split-Path -Leaf $dllPath
        Invoke-CheckedCommand -FilePath $script:Regsvr32Path -ArgumentList @('/s', $dllPath) -WorkingDirectory $SystemDirectory -Description "Registering $dllName"
    }
}

function Uninstall-ProxyStubDlls {
    param(
        [Parameter(Mandatory = $true)]
        [string]$SystemDirectory,

        [Parameter(Mandatory = $true)]
        [string]$RegistrationDirectory
    )

    foreach ($dllName in @('opccomn_ps.dll', 'opcproxy.dll')) {
        $destination = Join-Path $SystemDirectory $dllName
        $registrationPath = Join-Path $RegistrationDirectory $dllName

        if (-not (Test-Path -LiteralPath $destination)) {
            Write-Host "  Skipping $dllName; not present in $RegistrationDirectory"
            continue
        }

        $markerHash = Get-ProxyStubInstallMarkerHash -DllName $dllName
        if (-not $markerHash) {
            Write-Warning "Skipping $dllName removal because this script did not record installing it."
            continue
        }

        $currentHash = (Get-FileHash -LiteralPath $destination -Algorithm SHA256).Hash
        if (-not [string]::Equals($currentHash, $markerHash, [StringComparison]::OrdinalIgnoreCase)) {
            Write-Warning "Skipping $dllName removal because the System32 copy no longer matches the file installed by this script."
            continue
        }

        try {
            Invoke-CheckedCommand -FilePath $script:Regsvr32Path -ArgumentList @('/u', '/s', $registrationPath) -WorkingDirectory $SystemDirectory -Description "Unregistering $dllName"
        }
        catch {
            Write-Warning $_.Exception.Message
        }

        Write-Host "  Deleting $registrationPath"
        Remove-Item -LiteralPath $destination -Force -ErrorAction Stop
    }
}

function Set-TestServerRegistryEntries {
    param(
        [Parameter(Mandatory = $true)]
        [string]$ResolvedExePath
    )

    $description = 'OPC DA 2.05a Test Server (x64)'

    Invoke-RegExe -ArgumentList @('add', "HKLM\SOFTWARE\Classes\CLSID\$script:Clsid", '/ve', '/t', 'REG_SZ', '/d', $description, '/f')
    Invoke-RegExe -ArgumentList @('add', "HKLM\SOFTWARE\Classes\CLSID\$script:Clsid", '/v', 'AppID', '/t', 'REG_SZ', '/d', $script:Clsid, '/f')
    Invoke-RegExe -ArgumentList @('add', "HKLM\SOFTWARE\Classes\CLSID\$script:Clsid\LocalServer32", '/ve', '/t', 'REG_SZ', '/d', "`"$ResolvedExePath`"", '/f')
    Invoke-RegExe -ArgumentList @('add', "HKLM\SOFTWARE\Classes\CLSID\$script:Clsid\ProgID", '/ve', '/t', 'REG_SZ', '/d', $script:ProgId, '/f')
    Invoke-RegExe -ArgumentList @('add', "HKLM\SOFTWARE\Classes\CLSID\$script:Clsid\VersionIndependentProgID", '/ve', '/t', 'REG_SZ', '/d', $script:ProgIdAny, '/f')
    Invoke-RegExe -ArgumentList @('add', 'HKLM\SOFTWARE\Classes\AppID\OpcTestServer_x64.exe', '/ve', '/t', 'REG_SZ', '/d', $script:Clsid, '/f')
    Invoke-RegExe -ArgumentList @('add', "HKLM\SOFTWARE\Classes\AppID\$script:Clsid", '/ve', '/t', 'REG_SZ', '/d', $description, '/f')

    foreach ($catid in @($script:Catid10, $script:Catid20, $script:Catid30)) {
        Invoke-RegExe -ArgumentList @('add', "HKLM\SOFTWARE\Classes\CLSID\$script:Clsid\Implemented Categories\$catid", '/f')
    }

    $progIdPairs = @(
        @{ Versioned = $script:ProgId; VersionIndependent = $script:ProgIdAny },
        @{ Versioned = $script:UpstreamProgId; VersionIndependent = $script:UpstreamProgIdAny }
    )

    foreach ($pair in $progIdPairs) {
        Invoke-RegExe -ArgumentList @('add', "HKLM\SOFTWARE\Classes\$($pair.Versioned)", '/ve', '/t', 'REG_SZ', '/d', $description, '/f')
        Invoke-RegExe -ArgumentList @('add', "HKLM\SOFTWARE\Classes\$($pair.Versioned)\CLSID", '/ve', '/t', 'REG_SZ', '/d', $script:Clsid, '/f')
        Invoke-RegExe -ArgumentList @('add', "HKLM\SOFTWARE\Classes\$($pair.VersionIndependent)", '/ve', '/t', 'REG_SZ', '/d', $description, '/f')
        Invoke-RegExe -ArgumentList @('add', "HKLM\SOFTWARE\Classes\$($pair.VersionIndependent)\CLSID", '/ve', '/t', 'REG_SZ', '/d', $script:Clsid, '/f')
        Invoke-RegExe -ArgumentList @('add', "HKLM\SOFTWARE\Classes\$($pair.VersionIndependent)\CurVer", '/ve', '/t', 'REG_SZ', '/d', $pair.Versioned, '/f')
    }
}

if (-not (Test-IsAdministrator)) {
    Write-Error 'This script must be run from an elevated PowerShell window.'
    exit 1
}

if (-not [Environment]::Is64BitOperatingSystem) {
    Write-Error 'OpcTestServer_x64.exe registration requires a 64-bit Windows host.'
    exit 1
}

if (-not [Environment]::Is64BitProcess) {
    Write-Error 'OpcTestServer_x64.exe registration must be run from 64-bit PowerShell. Use %SystemRoot%\System32\WindowsPowerShell\v1.0\powershell.exe.'
    exit 1
}

$script:Clsid            = '{F8582CF9-88FB-11DA-A5ED-0060B0692061}'
$script:ProgId           = 'OpcTestServer_x64.1'
$script:ProgIdAny        = 'OpcTestServer_x64'
$script:UpstreamProgId   = 'OPC.OpcTestServer_x64.1'
$script:UpstreamProgIdAny = 'OPC.OpcTestServer_x64'
$script:Catid10          = '{63D5F430-CFE4-11D1-B2C8-0060083BA1FB}'
$script:Catid20          = '{63D5F432-CFE4-11D1-B2C8-0060083BA1FB}'
$script:Catid30          = '{CC603642-66D7-48F1-B69A-B625E73652D7}'
$script:InstallMarkerRegPath = 'HKLM\SOFTWARE\Opc.Classic\TestServerNoMsi'
$script:InstallMarkerProviderPath = 'HKLM:\SOFTWARE\Opc.Classic\TestServerNoMsi'

$system32Directory = Resolve-System32Directory
$nativeSystemDirectory = Resolve-NativeSystemDirectory
$script:RegExePath = Join-Path $nativeSystemDirectory 'reg.exe'
$script:Regsvr32Path = Join-Path $nativeSystemDirectory 'regsvr32.exe'

if (-not (Test-Path -LiteralPath $script:RegExePath)) {
    Write-Error "Cannot find native reg.exe at $script:RegExePath."
    exit 1
}

if (-not (Test-Path -LiteralPath $script:Regsvr32Path)) {
    Write-Error "Cannot find native regsvr32.exe at $script:Regsvr32Path."
    exit 1
}

if (-not $ExePath) {
    $ExePath = Resolve-DefaultTestServerPath
}

if ($ExePath -and (Test-Path -LiteralPath $ExePath)) {
    $ExePath = (Resolve-Path -LiteralPath $ExePath).Path
}
elseif (-not $Unregister) {
    Write-Error 'Cannot find OpcTestServer_x64.exe. Run tools\build-testserver.ps1 first, or pass -ExePath explicitly.'
    exit 1
}
else {
    Write-Warning 'OpcTestServer_x64.exe was not found; removing registry entries without running /unregserver.'
    $ExePath = $null
}

if ($Unregister) {
    Write-Host 'Unregistering TestServer from HKLM...'

    if ($ExePath) {
        try {
            Invoke-CheckedCommand -FilePath $ExePath -ArgumentList @('/unregserver') -WorkingDirectory $nativeSystemDirectory -Description 'Running OpcTestServer_x64.exe /unregserver'
        }
        catch {
            Write-Warning $_.Exception.Message
        }
    }

    foreach ($key in @(
        "HKLM\SOFTWARE\Classes\CLSID\$script:Clsid",
        "HKLM\SOFTWARE\Classes\$script:ProgId",
        "HKLM\SOFTWARE\Classes\$script:ProgIdAny",
        "HKLM\SOFTWARE\Classes\$script:UpstreamProgId",
        "HKLM\SOFTWARE\Classes\$script:UpstreamProgIdAny",
        "HKLM\SOFTWARE\Classes\AppID\OpcTestServer_x64.exe",
        "HKLM\SOFTWARE\Classes\AppID\$script:Clsid"
    )) {
        Remove-RegKey -KeyPath $key
    }

    Uninstall-ProxyStubDlls -SystemDirectory $nativeSystemDirectory -RegistrationDirectory $system32Directory
    Remove-RegKey -KeyPath $script:InstallMarkerRegPath

    Write-Host 'Done. Removed TestServer entries and any copied proxy-stub DLLs found in System32.'
    exit 0
}

$artifactDirectory = Split-Path -Parent $ExePath
$bareClsid = $script:Clsid.Trim('{}')

Write-Host 'Registering TestServer to HKLM...'
Write-Host "  EXE:    $ExePath"
Write-Host "  DLLs:   $artifactDirectory"
Write-Host "  System: $system32Directory"
Write-Host "  CLSID:  $script:Clsid"
Write-Host "  ProgID: $script:ProgId"

Install-ProxyStubDlls -ArtifactDirectory $artifactDirectory -SystemDirectory $nativeSystemDirectory -RegistrationDirectory $system32Directory
Invoke-CheckedCommand -FilePath $ExePath -ArgumentList @('/regserver') -WorkingDirectory $nativeSystemDirectory -Description 'Running OpcTestServer_x64.exe /regserver'
Set-TestServerRegistryEntries -ResolvedExePath $ExePath

Write-Host 'Done. Verify with:'
Write-Host "  python tools\probe_servers.py --da-clsid $bareClsid --da-browse-branch Test --da-read-item Test.Int32 --da-bucket-int-item Test.Int32 --da-bucket-string-item Test.String"
Write-Host '  python mcp\mcp_driver.py --testserver'
