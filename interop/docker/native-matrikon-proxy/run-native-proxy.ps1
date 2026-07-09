# SPDX-License-Identifier: MIT
# Copyright (c) 2026 Opc.Classic .NET Contributors

[CmdletBinding()]
param(
    [string] $TargetHost = $env:OPC_CLASSIC_TARGET_HOST,
    [string] $ProgId = $(if ($env:OPC_CLASSIC_TARGET_PROGID) { $env:OPC_CLASSIC_TARGET_PROGID } else { 'Opc.Classic.Simulation.DA.1' }),
    [string] $ItemId = $(if ($env:OPC_CLASSIC_TARGET_ITEMID) { $env:OPC_CLASSIC_TARGET_ITEMID } else { 'Plant.Reactor1.Temperature' }),
    [switch] $Interactive
)

$ErrorActionPreference = 'Stop'
$exe = 'C:/client/opc-test.exe'
if (-not (Test-Path -LiteralPath $exe)) {
    throw "Missing native exerciser: $exe"
}

if (-not $TargetHost) {
    Write-Warning 'Set OPC_CLASSIC_TARGET_HOST to the Linux SimulationServer host or pass -TargetHost.'
    if ($Interactive) { while ($true) { Start-Sleep -Seconds 30 } }
    exit 2
}

Write-Host "Native OPC DA Matrikon-proxy smoke: $ProgId on $TargetHost item $ItemId"
Write-Host 'Credentials are supplied to COM/DCOM by the container account or host DCOM policy; match them to OPC_CLASSIC_DCOM_USER/PASSWORD/DOMAIN on the managed server.'
& $exe $ProgId $TargetHost $ItemId
exit $LASTEXITCODE
