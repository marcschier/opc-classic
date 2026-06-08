#requires -Version 5.1
<#
.SYNOPSIS
    Register the OPC Foundation TestServer for DCOM activation.

.DESCRIPTION
    Performs the no-MSI registration steps that DCOM SCM needs to activate
    the OPC Foundation TestServer x64. The canonical reference is the
    legacy OPC-Classic-CoreComponents installer manifests; this script mirrors
    the required no-MSI registration without requiring msiexec. See
    docs/interop/testserver-registration-spec.md for the full audit.

    Concretely the script:
      1. Copies the full proxy/stub DLL set (opccomn_ps, opcproxy,
         opc_aeps, opcbc_ps, OpcCmdPs, OpcDxPs, opchda_ps, opcsec_ps)
         into %SystemRoot%\System32 and registers each with regsvr32.
         Registration order matters: opccomn_ps must come first so the
         dependent DLLs can resolve its TypeLib references.
      2. Copies OpcTestServer_x64.config.xml alongside the EXE
         (the TestServer reads it on startup; absence triggers
         CO_E_SERVER_EXEC_FAILURE during DCOM activation).
      3. Runs OpcCategoryManager.exe /RegServer for x64 category
         enumeration (used by CLSID-to-category resolvers).
      4. Runs OpcTestServer_x64.exe /regserver from %SystemRoot%\System32
         to write the EXE's own CLSID/ProgID/LocalServer32/TypeLib/
         AppID/Implemented-Categories entries.
      5. Writes compatibility CLSID, ProgID, LocalServer32, and DA
         category entries directly under HKLM\SOFTWARE\Classes to ensure
         a clean post-install state regardless of OPC_* macro
         expansion variations.

    The DCOM Service Control Manager runs as SYSTEM and does NOT honor
    per-user HKCU registrations for LocalServer32 activation — only
    HKLM entries are visible. Hence this script requires admin
    elevation from 64-bit PowerShell.

.PARAMETER ExePath
    Full path to OpcTestServer_x64.exe (default: looks for it under
    external\redist\build\x64\Release\, the vendored CMake output
    produced by tools\build-testserver.ps1). The sibling proxy/stub
    DLLs and OpcTestServer_x64.config.xml are expected alongside.

.PARAMETER Unregister
    Run OpcTestServer_x64.exe /unregserver when the EXE is available,
    remove the TestServer HKLM entries, unregister OpcCategoryManager.exe,
    unregister the copied proxy-stub DLLs from System32, and delete them
    if present.

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
    $candidate = Join-Path $repoRoot 'external\redist\build\x64\Release\OpcTestServer_x64.exe'
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

    # Register the FULL canonical proxy/stub DLL set per the legacy upstream
    # installer manifest order. Order matters: opccomn_ps must
    # be registered first because the other DLLs reference its IOPCCommon/
    # IOPCShutdown IIDs via TypeLib imports. See:
    # docs/interop/testserver-registration-spec.md
    $dllNames = @(
        'opccomn_ps.dll',
        'opcproxy.dll',
        'opc_aeps.dll',
        'opcbc_ps.dll',
        'OpcCmdPs.dll',
        'OpcDxPs.dll',
        'opchda_ps.dll',
        'opcsec_ps.dll'
    )
    $installedDlls = @()

    foreach ($dllName in $dllNames) {
        $source = Join-Path $ArtifactDirectory $dllName
        if (-not (Test-Path -LiteralPath $source)) {
            Write-Warning "Skipping $dllName; not present in $ArtifactDirectory. Re-run tools\build-testserver.ps1 to produce the full proxy/stub set; DA-only marshalling needs opccomn_ps + opcproxy at a minimum."
            continue
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

    # Unregister in reverse install order. See Install-ProxyStubDlls for the
    # rationale: dependent DLLs must come down before the foundational
    # opccomn_ps so the TypeLib references are still resolvable until each
    # DllUnregisterServer call completes.
    $dllNames = @(
        'opcsec_ps.dll',
        'opchda_ps.dll',
        'OpcDxPs.dll',
        'OpcCmdPs.dll',
        'opcbc_ps.dll',
        'opc_aeps.dll',
        'opcproxy.dll',
        'opccomn_ps.dll'
    )

    foreach ($dllName in $dllNames) {
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

function Copy-TestServerConfig {
    param(
        [Parameter(Mandatory = $true)]
        [string]$ArtifactDirectory,

        [Parameter(Mandatory = $true)]
        [string]$ExeDirectory
    )

    # The CoreComponents install rules deploy OpcTestServer_x64.config.xml
    # alongside the EXE. The TestServer reads this
    # file on startup (loaded by COpcTestServer init); absence may cause the
    # EXE to fail before it registers a class factory, producing
    # CO_E_SERVER_EXEC_FAILURE during DCOM activation.
    #
    # The CMake build only copies/renames this file via `cmake --install` (rule
    # at external/redist/CMakeLists.txt); a plain `cmake --build` step
    # leaves the source file at external/redist/samples/OpcTestServer/
    # OpcTestServer.config.xml. Look for it in both locations.
    $configName = 'OpcTestServer_x64.config.xml'
    $destination = Join-Path $ExeDirectory $configName

    $candidates = @(
        (Join-Path $ArtifactDirectory $configName),
        (Join-Path $PSScriptRoot '..\external\redist\samples\OpcTestServer\OpcTestServer.config.xml')
    )

    foreach ($candidate in $candidates) {
        if (Test-Path -LiteralPath $candidate) {
            $resolved = (Resolve-Path -LiteralPath $candidate).Path
            $alreadyInPlace = ($resolved -ieq (Resolve-Path -LiteralPath $destination -ErrorAction SilentlyContinue).Path)
            if (-not $alreadyInPlace) {
                Write-Host "  Copying $configName from $resolved to $ExeDirectory"
                Copy-Item -LiteralPath $resolved -Destination $destination -Force
            } else {
                Write-Host "  $configName already in place beside the EXE."
            }
            # OPC Foundation TestServer source bug: the auto-generated
            # <SelfRegInfo>/<CLSID> element written by /regserver uses the
            # IDL coclass UUID F8582CF8-... but the runtime CoRegisterClassObject
            # uses the OPC_IMPLEMENT_LOCAL_SERVER UUID F8582CF9-... Without
            # this patch the class factory registers under the WRONG CLSID
            # (the coclass UUID) and SCM activation of the registered
            # F8582CF9 CLSID times out with CO_E_SERVER_EXEC_FAILURE.
            # See docs/interop/testserver-registration-spec.md for the full
            # diagnostic trace.
            if (Test-Path -LiteralPath $destination) {
                $content = Get-Content -Raw -LiteralPath $destination
                $patched = $content -replace '<CLSID>\{F8582CF8-88FB-11DA-A5ED-0060B0692061\}</CLSID>', '<CLSID>{F8582CF9-88FB-11DA-A5ED-0060B0692061}</CLSID>'
                if ($patched -ne $content) {
                    Write-Host "  Patching <CLSID> in ${configName}: F8582CF8 -> F8582CF9 (OPC_IMPLEMENT_LOCAL_SERVER alignment)"
                    # [IO.File]::WriteAllText (vs Set-Content -NoNewline) avoids a
                    # trailing newline insertion that Set-Content adds on PS 5.1.
                    [System.IO.File]::WriteAllText($destination, $patched)
                }
            }
            return
        }
    }

    Write-Warning "Skipping $configName; not present in $ArtifactDirectory or the vendored source tree. Without it, the TestServer may fail to initialize on activation (CO_E_SERVER_EXEC_FAILURE)."
}

function Register-OpcCategoryManager {
    param(
        [Parameter(Mandatory = $true)]
        [string]$ArtifactDirectory
    )

    # CoreComponents registers OpcCategoryManager.exe as a COM local server via
    # /RegServer. It handles x64 category-presence
    # enumeration (CATID_OPCDAServer*) that some clients rely on when
    # resolving CLSID-to-category mappings.
    $exeName = 'OpcCategoryManager.exe'
    $exePath = Join-Path $ArtifactDirectory $exeName

    if (-not (Test-Path -LiteralPath $exePath)) {
        Write-Warning "Skipping $exeName /RegServer; not present in $ArtifactDirectory."
        return
    }

    try {
        Invoke-CheckedCommand -FilePath $exePath -ArgumentList @('/RegServer') -Description "Running $exeName /RegServer"
    }
    catch {
        Write-Warning $_.Exception.Message
    }
}

function Unregister-OpcCategoryManager {
    param(
        [Parameter(Mandatory = $true)]
        [string]$ArtifactDirectory
    )

    $exeName = 'OpcCategoryManager.exe'
    $exePath = Join-Path $ArtifactDirectory $exeName

    if (-not (Test-Path -LiteralPath $exePath)) {
        return
    }

    try {
        Invoke-CheckedCommand -FilePath $exePath -ArgumentList @('/UnRegServer') -Description "Running $exeName /UnRegServer"
    }
    catch {
        Write-Warning $_.Exception.Message
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
    if ($ExePath) {
        Unregister-OpcCategoryManager -ArtifactDirectory (Split-Path -Parent $ExePath)
    }
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
Copy-TestServerConfig -ArtifactDirectory $artifactDirectory -ExeDirectory $artifactDirectory
Register-OpcCategoryManager -ArtifactDirectory $artifactDirectory
Invoke-CheckedCommand -FilePath $ExePath -ArgumentList @('/regserver') -WorkingDirectory $nativeSystemDirectory -Description 'Running OpcTestServer_x64.exe /regserver'
Set-TestServerRegistryEntries -ResolvedExePath $ExePath

Write-Host 'Done. Verify with:'
Write-Host "  python tools\probe_servers.py --da-clsid $bareClsid --da-browse-branch Test --da-read-item Test.Int32 --da-bucket-int-item Test.Int32 --da-bucket-string-item Test.String"
Write-Host '  python mcp\mcp_driver.py --testserver'
