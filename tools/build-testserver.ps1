#requires -Version 5.1
<#
.SYNOPSIS
    Build the OPC Foundation TestServer (x64) from the vendored
    ext/redist/CoreComponents/ tree using CMake + MSVC.

.DESCRIPTION
    Wraps the upstream OPC-Classic-CoreComponents CMake harness so the
    native TestServer + supporting proxy/stub DLLs can be built without
    an external clone. Produces (under ext/redist/CoreComponents/build/x64/Release/):
      OpcTestServer_x64.exe + OpcTestServer_x64.config.xml
      OpcTestClient_x64.exe
      OpcCategoryManager.exe
      opccomn_ps.dll, opcproxy.dll, opc_aeps.dll, opcbc_ps.dll,
      OpcCmdPs.dll, OpcDxPs.dll, opchda_ps.dll, opcsec_ps.dll
      (matches the full proxy/stub set that the CoreComponents install rules deploy;
      see docs\interop\testserver-registration-spec.md)

    Prerequisites: Visual Studio 2022 17.14+ (Desktop development with
    C++ + ATL) and CMake 3.20+. CMake is shipped with VS - the script
    falls back to it if needed.

.PARAMETER Configuration
    Release (default) or Debug.

.PARAMETER Clean
    Wipe the build/ directory before configuring.

.EXAMPLE
    .\tools\build-testserver.ps1
    .\tools\build-testserver.ps1 -Clean
#>

[CmdletBinding()]
param(
    [ValidateSet('Release','Debug')]
    [string]$Configuration = 'Release',
    [switch]$Clean
)

$ErrorActionPreference = 'Stop'
$repoRoot = Split-Path -Parent $PSScriptRoot
$sourceDir = Join-Path $repoRoot 'ext\redist\CoreComponents'
$buildDir  = Join-Path $sourceDir 'build\x64'
$samplesDir = Join-Path $repoRoot 'samples'

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
    Write-Host "Configuring CMake (x64) ..."
    & $cmake -S . -B $buildDir -A x64 "-DOPC_TEST_SAMPLES_DIR=$samplesDir"
    if ($LASTEXITCODE -ne 0) { throw "cmake configure failed (exit $LASTEXITCODE)" }

    Write-Host "Building CMake project ($Configuration) ..."
    # Build the full proxy/stub set so the tools\register-testserver.ps1 script
    # has all 8 DLLs available to register (matches what the CoreComponents
    # install rules deploy; see docs\interop\testserver-registration-spec.md).
    & $cmake --build $buildDir --config $Configuration --target `
        OpcTestServer OpcTestClient OpcCategoryManager `
        opccomn_ps opcproxy opc_aeps opcbc_ps OpcCmdPs OpcDxPs opchda_ps opcsec_ps
    if ($LASTEXITCODE -ne 0) { throw "cmake build failed (exit $LASTEXITCODE)" }

    $exe = Join-Path $buildDir "$Configuration\OpcTestServer_x64.exe"
    if (Test-Path $exe) {
        Write-Host "Build succeeded. Artifact: $exe"
        Write-Host "Next: register with 'tools\register-testserver.ps1' (elevated)."
    } else {
        Write-Warning "Build completed but $exe not found."
    }
}
finally {
    Pop-Location
}
