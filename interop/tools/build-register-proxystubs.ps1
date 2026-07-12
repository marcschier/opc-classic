#requires -Version 5.1
<#
.SYNOPSIS
    Build and register the OPC Foundation interface proxy/stub DLLs (x64 + x86)
    from the vendored interop/ tree using CMake + MSVC.

.DESCRIPTION
    OPC Classic servers expose their interfaces (IOPCServer, IOPCCommon,
    IOPCBrowse, IOPCItemIO, IOPCSecurity*, IOPCEventServer, IOPCHDA_Server, ...)
    over DCOM. When a server is hosted by real Windows COM and activated by the
    real SCM/RPCSS, the real combase runtime marshals those interface pointers
    using *standard marshaling*, which requires the matching OPC interface
    proxy/stub DLL to be registered under
    HKCR\Interface\{IID}\ProxyStubClsid32 on the host.

    A developer box with OPC Core Components installed already has these
    (C:\Windows\System32\opcproxy.dll etc.), which is why cross-impl activation
    works locally. A clean CI runner does NOT, so combase cannot marshal the
    managed sample servers' OPC interfaces and RPCSS returns E_NOINTERFACE
    (0x80004002) for every activation. This script closes that gap by building
    and registering the vendored proxy/stubs.

    Bitness:
      * x64 sample servers (Opc.Classic.Samples.*Server run AnyCPU => 64-bit)
        need the x64 proxy/stubs.
      * The vendored OpcEnum service is x86; IOPCServerList/2 marshaling out of
        that process needs the x86 opccomn_ps.

    Prerequisites: Visual Studio 2022 17.14+ (Desktop development with C++ + ATL)
    and CMake 3.20+ (bundled with VS). Registration requires elevation.

.PARAMETER Configuration
    Release (default) or Debug.

.PARAMETER Clean
    Wipe the build directories before configuring.

.EXAMPLE
    .\interop\tools\build-register-proxystubs.ps1
#>

[CmdletBinding()]
param(
    [ValidateSet('Release', 'Debug')]
    [string]$Configuration = 'Release',
    [switch]$Clean
)

$ErrorActionPreference = 'Stop'
$repoRoot = Split-Path -Parent (Split-Path -Parent $PSScriptRoot)
$sourceDir = Join-Path $repoRoot 'interop'

if (-not (Test-Path $sourceDir)) {
    Write-Error "Vendored CoreComponents not found at $sourceDir"
    exit 1
}

# Discover CMake (prefer VS's bundled CMake to match the upstream's tested
# combination, fall back to PATH).
$cmake = $null
$vswhere = "${env:ProgramFiles(x86)}\Microsoft Visual Studio\Installer\vswhere.exe"
if (Test-Path $vswhere) {
    $vsRoot = & $vswhere -latest -products * -requires Microsoft.VisualStudio.Component.VC.CMake.Project -property installationPath 2>$null | Select-Object -First 1
    if ($vsRoot) {
        $candidate = Join-Path $vsRoot 'Common7\IDE\CommonExtensions\Microsoft\CMake\CMake\bin\cmake.exe'
        if (Test-Path $candidate) { $cmake = $candidate }
    }
}
if (-not $cmake) {
    $cmake = (Get-Command cmake.exe -ErrorAction SilentlyContinue).Source
}
if (-not $cmake) {
    Write-Error 'cmake.exe not found. Install Visual Studio 2022 with C++ + CMake, or add cmake to PATH.'
    exit 1
}
Write-Host "Using cmake: $cmake"

# Proxy/stub targets required by the cross-impl matrix profiles:
#   opccomn_ps -> IOPCCommon, IOPCServerList/2   (all profiles + discovery)
#   opcproxy   -> IOPCServer, IOPCBrowse*, IOPCItemIO, IOPCItemProperties (DA)
#   opcsec_ps  -> IOPCSecurityNT / IOPCSecurityPrivate (security-da)
#   opc_aeps   -> IOPCEventServer / IOPCEventSubscriptionMgmt (AE)
#   opchda_ps  -> IOPCHDA_Server / IOPCHDA_Browser (HDA)
$x64Targets = @('opccomn_ps', 'opcproxy', 'opcsec_ps', 'opc_aeps', 'opchda_ps')
# The vendored OpcEnum service is x86 and marshals IOPCServerList out of its own
# 32-bit process, so it needs the x86 opccomn_ps proxy/stub registered too.
$x86Targets = @('opccomn_ps')

function Build-And-Register {
    param(
        [Parameter(Mandatory)] [ValidateSet('x64', 'x86')] [string]$Arch,
        [Parameter(Mandatory)] [string[]]$Targets
    )

    $cmakeArch = if ($Arch -eq 'x64') { 'x64' } else { 'Win32' }
    $buildDir = Join-Path $sourceDir "build\$Arch"
    if ($Clean -and (Test-Path $buildDir)) {
        Write-Host "Removing $buildDir ..."
        Remove-Item -Path $buildDir -Recurse -Force
    }

    Push-Location $sourceDir
    try {
        Write-Host "=== [$Arch] Configuring CMake ($cmakeArch) ==="
        & $cmake -S . -B $buildDir -A $cmakeArch -DOPC_BUILD_TESTS=OFF
        if ($LASTEXITCODE -ne 0) { throw "[$Arch] cmake configure failed (exit $LASTEXITCODE)" }

        Write-Host "=== [$Arch] Building proxy/stubs: $($Targets -join ', ') ==="
        & $cmake --build $buildDir --config $Configuration --target @Targets
        if ($LASTEXITCODE -ne 0) { throw "[$Arch] cmake build failed (exit $LASTEXITCODE)" }

        $regsvr = if ($Arch -eq 'x64') {
            Join-Path $env:WINDIR 'System32\regsvr32.exe'
        } else {
            Join-Path $env:WINDIR 'SysWOW64\regsvr32.exe'
        }

        foreach ($target in $Targets) {
            $dll = Join-Path $buildDir "$Configuration\$target.dll"
            if (-not (Test-Path $dll)) {
                throw "[$Arch] built proxy/stub not found: $dll"
            }
            Write-Host "=== [$Arch] regsvr32 $dll ==="
            $p = Start-Process $regsvr -ArgumentList '/s', "`"$dll`"" -Wait -PassThru -NoNewWindow
            if ($p.ExitCode -ne 0) {
                throw "[$Arch] regsvr32 failed for $dll (exit $($p.ExitCode))"
            }
        }
        Write-Host "=== [$Arch] registered $($Targets.Count) proxy/stub(s) ==="
    }
    finally {
        Pop-Location
    }
}

Build-And-Register -Arch x64 -Targets $x64Targets
Build-And-Register -Arch x86 -Targets $x86Targets

Write-Host "OPC interface proxy/stubs built and registered (x64 + x86)."
