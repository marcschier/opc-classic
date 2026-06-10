# SPDX-License-Identifier: MIT
# Copyright (c) 2026 Opc.Classic .NET Contributors
#
# Entry point for the Windows-container variant of Opc.Classic.Samples.CttServer.
# Registers the server under HKLM (both registry views) and then runs the
# managed hosted-service in the foreground. Container exits when the service
# exits; --unregister is called on graceful shutdown.

$ErrorActionPreference = 'Stop'
$exe = 'C:/app/Opc.Classic.Samples.CttServer.exe'

Write-Host '== opc-classic/managed init =='

# Start OPCEnum so OPC clients on the network can browse for our server.
$opcEnum = Get-Service -Name OpcEnum -ErrorAction SilentlyContinue
if ($opcEnum -and $opcEnum.Status -ne 'Running') {
    Start-Service OpcEnum
    Write-Host 'Started OpcEnum service'
}

Write-Host '-- Registering Opc.Classic.DaSample.1 under HKLM, both registry views'
& $exe --register --registry-hive=hklm
if ($LASTEXITCODE -ne 0) {
    throw "--register failed with exit code $LASTEXITCODE"
}

try {
    Write-Host '-- Running managed DA sample server'
    & $exe
}
finally {
    Write-Host '-- Unregistering on shutdown'
    & $exe --unregister --registry-hive=hklm
}
