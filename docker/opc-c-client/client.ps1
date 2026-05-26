# SPDX-License-Identifier: MIT
# Copyright (c) 2026 Opc.Classic .NET Contributors
#
# ENTRYPOINT for opc-classic/c-client. Drives the OPCTEST.exe console smoke
# against a target ProgID on a peer container, then exits with the test's
# exit code.

[CmdletBinding()]
param(
    [string] $ProgId = 'OPC.SampleServer.1',
    [string] $TargetHost,
    [string] $Tool = 'opc-test.exe',
    [switch] $Speed,
    [switch] $Interactive
)

$ErrorActionPreference = 'Stop'

$exe = if ($Speed) { 'C:/client/opc-speed.exe' } else { "C:/client/$Tool" }

if (-not (Test-Path $exe)) {
    Write-Warning "$exe is missing; this image is a Phase-3 scaffold."
    if ($Interactive) {
        Write-Host 'Sleeping for `docker exec` access.'
        while ($true) { Start-Sleep -Seconds 30 }
    } else {
        exit 1
    }
}

Write-Host "Invoking $exe against $ProgId on $TargetHost"
$args = @($ProgId)
if ($TargetHost) { $args += $TargetHost }
& $exe @args
exit $LASTEXITCODE
