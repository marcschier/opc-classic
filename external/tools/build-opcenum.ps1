#requires -Version 5.1
<#
.SYNOPSIS
    Build the OPC Foundation OpcEnum service (x86) from the vendored
    external/ tree using CMake + MSVC.

.DESCRIPTION
    OpcEnum.exe is the OPC Foundation server-enumeration service used
    by OPC Classic clients to browse for installed servers via DCOM.
    The CMake project at external/CMakeLists.txt declares an
    x86-only `OpcEnum` target built from
    external/src/Common/ServerEnumerator/. This helper wraps
    the configure + build step and produces:

      external/build/x86/Release/OpcEnum.exe

    Registration (run elevated, after build):

      OpcEnum.exe /Service     # install as Windows service (CreateService + LocalServer32 + LocalService=OpcEnum)
      Start-Service OpcEnum    # start the service
      OpcEnum.exe /UnregServer # uninstall

    OpcEnum.exe is x86-only because it's a vendored OPC Foundation
    binary written against the original 32-bit COM type library; the
    OpcServerList interface (IOPCServerList / IOPCServerList2) is
    routinely consumed by 32-bit OPC clients, and a 64-bit OpcEnum
    would break compatibility with that majority of the ecosystem.
    DCOM Local Activation transparently bridges the 32-bit OpcEnum
    service into 64-bit client processes via the SCM.

    Prerequisites: Visual Studio 2022 17.14+ (Desktop development with
    C++ + ATL) and CMake 3.20+. CMake is shipped with VS - the script
    falls back to it if needed.

    This replaces the previous workflow that installed OpcEnum via the
    OPC Compliance Test Tool Common Modules MSI (which is no longer
    vendored under external/private/ctt/).

.PARAMETER Configuration
    Release (default) or Debug.

.PARAMETER Clean
    Wipe the x86 build directory before configuring.

.EXAMPLE
    .\external\tools\build-opcenum.ps1
    .\external\tools\build-opcenum.ps1 -Clean
#>

[CmdletBinding()]
param(
    [ValidateSet('Release','Debug')]
    [string]$Configuration = 'Release',
    [switch]$Clean
)

$ErrorActionPreference = 'Stop'
$repoRoot = Split-Path -Parent (Split-Path -Parent $PSScriptRoot)
$sourceDir = Join-Path $repoRoot 'external'
$buildDir  = Join-Path $sourceDir 'build\x86'

if (-not (Test-Path $sourceDir)) {
    Write-Error "Vendored CoreComponents not found at $sourceDir"
    exit 1
}

if ($Clean -and (Test-Path $buildDir)) {
    Write-Host "Removing $buildDir ..."
    Remove-Item -Path $buildDir -Recurse -Force
}

# Discover CMake (prefer VS's bundled CMake to match the upstream's
# tested combination, fall back to PATH).
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

Push-Location $sourceDir
try {
    Write-Host "Configuring CMake (x86 / Win32 platform) ..."
    & $cmake -S . -B $buildDir -A Win32
    if ($LASTEXITCODE -ne 0) { throw "cmake configure failed (exit $LASTEXITCODE)" }

    Write-Host "Building OpcEnum + opccomn_ps ($Configuration) ..."
    # OpcEnum needs opccomn_ps's MIDL output (opccomn.h) at compile time
    # because IOPCServerList/2 are defined under the OPC Common interface
    # set. Build both targets together; opccomn_ps is small (<100 KB).
    & $cmake --build $buildDir --config $Configuration --target OpcEnum opccomn_ps
    if ($LASTEXITCODE -ne 0) { throw "cmake build failed (exit $LASTEXITCODE)" }

    $exe = Join-Path $buildDir "$Configuration\OpcEnum.exe"
    if (Test-Path $exe) {
        Write-Host "Build succeeded. Artifact: $exe"
        Write-Host "Next (run elevated): & '$exe' /Service ; Start-Service OpcEnum"
    } else {
        Write-Warning "Build completed but $exe not found."
    }
}
finally {
    Pop-Location
}
