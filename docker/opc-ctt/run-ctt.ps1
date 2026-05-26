# SPDX-License-Identifier: MIT
# Copyright (c) 2026 Opc.Classic .NET Contributors
#
# Test runner shim for the opc-classic/ctt container. Invokes OpcCtt.exe
# against a target server (registered on this machine or via OPCEnum
# discovery of a peer container) and uploads the conformance XML.
#
# Usage examples:
#   docker run --rm --network opc-test-net opc-classic/ctt `
#     -ProgId Opc.Classic.DaSample.1 -TargetHost opc-classic-managed
#
#   # Run a saved CTT script bundle
#   docker run --rm -v $PWD/scripts:c:/scripts `
#     --network opc-test-net opc-classic/ctt `
#     -ScriptPath c:/scripts/da-2.05a-full.xml -OutputPath c:/results/run-1.xml

[CmdletBinding()]
param(
    [string] $ProgId,
    [string] $TargetHost,
    [string] $ScriptPath,
    [string] $OutputPath = 'C:\results\ctt-results.xml',
    [int] $TimeoutSeconds = 1800,
    [switch] $Help
)

$ErrorActionPreference = 'Stop'

if (-not (Test-Path (Split-Path $OutputPath -Parent))) {
    New-Item -ItemType Directory -Path (Split-Path $OutputPath -Parent) -Force | Out-Null
}

# Locate OpcCtt.exe across the documented install paths.
$candidates = @(
    "$env:ProgramFiles\OPC Foundation\OPC Compliance Test Tool\OpcCtt.exe",
    "${env:ProgramFiles(x86)}\OPC Foundation\OPC Compliance Test Tool\OpcCtt.exe",
    "$env:ProgramFiles\OPC Foundation\OPC Foundation Compliance Test Tool\OpcCtt.exe",
    "${env:ProgramFiles(x86)}\OPC Foundation\OPC Foundation Compliance Test Tool\OpcCtt.exe"
)
$cttExe = $candidates | Where-Object { Test-Path $_ } | Select-Object -First 1
if (-not $cttExe) {
    throw "OpcCtt.exe not found in any expected install path. Did the MSI install succeed?"
}
Write-Host "Using $cttExe"

# Dump CLI help on the first run so the canonical headless flags are recorded
# in the run log (the exact /AUTO / /Output: syntax for v2.0.15 has not been
# verified end-to-end yet — see docker/opc-ctt/README.md).
if ($Help) {
    & $cttExe /?
    return
}

# Ensure OPCEnum is running so the CTT can browse for the target server.
$opcEnum = Get-Service -Name OpcEnum -ErrorAction SilentlyContinue
if ($opcEnum -and $opcEnum.Status -ne 'Running') {
    Start-Service OpcEnum
    Write-Host 'Started OpcEnum service'
}

# Build the argument list.
$args = @('/AUTO', "/Output:$OutputPath")
if ($ProgId)      { $args += "/ServerProgId:$ProgId" }
if ($TargetHost)  { $args += "/TargetHost:$TargetHost" }
if ($ScriptPath)  { $args += "/Script:$ScriptPath" }

Write-Host "Invoking OpcCtt.exe with: $args"
$process = Start-Process -FilePath $cttExe -ArgumentList $args -PassThru -NoNewWindow
$exited = $process.WaitForExit([math]::Max(1, $TimeoutSeconds) * 1000)
if (-not $exited) {
    Write-Warning "OpcCtt.exe did not exit within $TimeoutSeconds seconds; killing."
    $process.Kill()
    throw 'OpcCtt.exe timed out'
}
Write-Host "OpcCtt.exe exited with code $($process.ExitCode)"

if (Test-Path $OutputPath) {
    Write-Host "Wrote results to $OutputPath ($((Get-Item $OutputPath).Length) bytes)"
} else {
    Write-Warning "No results file at $OutputPath"
}

exit $process.ExitCode
