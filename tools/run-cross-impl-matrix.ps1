# SPDX-License-Identifier: MIT
# Copyright (c) 2026 Opc.Classic .NET Contributors

<#
.SYNOPSIS
    One-shot automation for the cross-implementation interop matrix.

.DESCRIPTION
    Auto-registers every sample server (HKCU, no elevation) so the OPC
    Foundation OpcTestClient.exe can enumerate them, then runs the Python
    cross-impl-probe driver across every profile and emits a green/red
    summary. Optional -WireCapture enables Track CA `.pcap` capture per
    profile.

    Expectations:
      - .NET 10 SDK on PATH.
      - Python 3.10+ on PATH.
      - Sample servers built (`dotnet build` of samples/).
      - For TestServer profile: requires the TestServer DCOM ACL grant from
        `tools/grant-testserver-acl.ps1` to have run elevated once.

.PARAMETER Profile
    Restrict to specific profiles. May be repeated. Default: every profile.

.PARAMETER OutputDir
    Directory for matrix-out/ artifacts. Default: matrix-out/.

.PARAMETER SkipRegistration
    Skip the auto-register step (use when the samples are already
    registered elsewhere, e.g. HKLM via an elevated session).

.PARAMETER UseClsid
    Pass --use-clsid to the cross-impl driver (use CLSIDs instead of
    ProgIDs). Useful when OPCEnum is misbehaving on a hardened DCOM box.

.PARAMETER WireCapture
    Enable Track CA wire capture per profile. Artifacts land under
    matrix-out/wire-captures/<profile>/.

.PARAMETER RequestTimeoutSeconds
    Per-tool MCP request timeout. Default: 60.

.EXAMPLE
    .\tools\run-cross-impl-matrix.ps1

    Run every profile against localhost with default settings.

.EXAMPLE
    .\tools\run-cross-impl-matrix.ps1 -Profile samples-da -Profile samples-hda -WireCapture

    Only run the managed DA + HDA sample profiles, with wire capture
    artifacts written under matrix-out/wire-captures/.
#>

[CmdletBinding()]
param(
    [string[]]$Profile,
    [string]$OutputDir = "matrix-out",
    [switch]$SkipRegistration,
    [switch]$UseClsid,
    [switch]$WireCapture,
    [int]$RequestTimeoutSeconds = 60
)

$ErrorActionPreference = "Stop"

$RepoRoot = Resolve-Path (Join-Path $PSScriptRoot "..")
Push-Location $RepoRoot

try {
    # --- pre-flight ---

    if (-not (Get-Command python -ErrorAction SilentlyContinue)) {
        throw "python (3.10+) not found on PATH."
    }
    if (-not (Get-Command dotnet -ErrorAction SilentlyContinue)) {
        throw "dotnet SDK not found on PATH."
    }

    # SampleProfile -> (exePath, sampleProgId) map. Lines up with the
    # PROFILE_TARGETS dict in tools/run_cross_impl_matrix.py and the
    # CLSIDs/ProgIDs defined in each sample's Program.cs.
    $sampleRegistrations = @(
        @{ Profile = "samples-da";  Project = "samples\Opc.Classic.Samples.DaServer\Opc.Classic.Samples.DaServer.csproj";  Exe = "samples\Opc.Classic.Samples.DaServer\bin\Debug\net10.0\Opc.Classic.Samples.DaServer.exe" }
        @{ Profile = "ctt-da";      Project = "samples\Opc.Classic.Samples.CttServer\Opc.Classic.Samples.CttServer.csproj"; Exe = "samples\Opc.Classic.Samples.CttServer\bin\Debug\net10.0\Opc.Classic.Samples.CttServer.exe" }
        @{ Profile = "samples-ae";  Project = "samples\Opc.Classic.Samples.AeServer\Opc.Classic.Samples.AeServer.csproj";  Exe = "samples\Opc.Classic.Samples.AeServer\bin\Debug\net10.0\Opc.Classic.Samples.AeServer.exe" }
        @{ Profile = "samples-hda"; Project = "samples\Opc.Classic.Samples.HdaServer\Opc.Classic.Samples.HdaServer.csproj"; Exe = "samples\Opc.Classic.Samples.HdaServer\bin\Debug\net10.0\Opc.Classic.Samples.HdaServer.exe" }
        @{ Profile = "security-da"; Project = "samples\Opc.Classic.Samples.OpcSecurityServer\Opc.Classic.Samples.OpcSecurityServer.csproj"; Exe = "samples\Opc.Classic.Samples.OpcSecurityServer\bin\Debug\net10.0\Opc.Classic.Samples.OpcSecurityServer.exe" }
    )

    if (-not $SkipRegistration) {
        Write-Host "=== HKCU registration of sample servers ===" -ForegroundColor Cyan
        foreach ($sample in $sampleRegistrations) {
            # If this profile is excluded by -Profile, skip its registration.
            if ($Profile -and ($Profile -notcontains $sample.Profile)) {
                continue
            }
            $exe = Join-Path $RepoRoot $sample.Exe
            if (-not (Test-Path $exe)) {
                Write-Host "  Building $($sample.Project)..." -ForegroundColor Yellow
                & dotnet build $sample.Project --nologo -v:m -p:RestoreSources=https://api.nuget.org/v3/index.json | Out-Host
                if ($LASTEXITCODE -ne 0) {
                    throw "Failed to build $($sample.Project)"
                }
            }
            Write-Host "  Registering $($sample.Profile) ($exe --register --registry-hive=hkcu)"
            & $exe --register --registry-hive=hkcu | Out-Host
            if ($LASTEXITCODE -ne 0) {
                throw "Registration failed for $($sample.Profile) (exit code $LASTEXITCODE)"
            }
        }
    } else {
        Write-Host "-- skipping HKCU registration (per -SkipRegistration) --" -ForegroundColor Yellow
    }

    # --- run the cross-impl driver ---

    Write-Host ""
    Write-Host "=== Running cross-impl matrix ===" -ForegroundColor Cyan
    $args = @(
        "tools\run_cross_impl_matrix.py",
        "--output-dir", $OutputDir,
        "--request-timeout", "$RequestTimeoutSeconds"
    )
    if ($Profile) {
        foreach ($p in $Profile) { $args += @("--profile", $p) }
    }
    if ($UseClsid) { $args += "--use-clsid" }
    if ($WireCapture) {
        $captureRoot = Join-Path $OutputDir "wire-captures"
        New-Item -ItemType Directory -Force -Path $captureRoot | Out-Null
        $args += @("--save-wire-payloads", $captureRoot)
    }

    & python @args
    $matrixExit = $LASTEXITCODE
    if ($matrixExit -ne 0) {
        Write-Host ""
        Write-Host "Cross-impl matrix reported regressions (exit $matrixExit)" -ForegroundColor Red
        if (-not $WireCapture) {
            Write-Host ""
            Write-Host "Tip: re-run with -WireCapture to capture per-tool .hex wire dumps" -ForegroundColor Yellow
            Write-Host "    (artifacts land under $OutputDir\wire-captures\<profile>\) so" -ForegroundColor Yellow
            Write-Host "    failures can be diagnosed offline via opcclassic.capture.decode_pdu" -ForegroundColor Yellow
            Write-Host "    or replayed through the test fixtures." -ForegroundColor Yellow
        }
    } else {
        Write-Host ""
        Write-Host "Cross-impl matrix completed cleanly (exit 0)" -ForegroundColor Green
    }
    exit $matrixExit

} finally {
    Pop-Location
}
