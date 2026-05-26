# SPDX-License-Identifier: MIT
# Copyright (c) 2026 Opc.Classic .NET Contributors

$ErrorActionPreference = 'Stop'

Write-Host '== opc-classic/c-server init =='

# Start OPCEnum so the CTT and other clients can browse for us.
$opcEnum = Get-Service -Name OpcEnum -ErrorAction SilentlyContinue
if ($opcEnum -and $opcEnum.Status -ne 'Running') {
    Start-Service OpcEnum
    Write-Host 'Started OpcEnum service'
}

if (-not (Test-Path 'C:/server/opc_exe.exe')) {
    Write-Warning 'opc_exe.exe is missing; this image is a Phase-2 scaffold. See docker/opc-c-server/build/README.md.'
    # Keep the container alive for `docker exec` debugging.
    while ($true) { Start-Sleep -Seconds 30 }
}

# Register the server with HKCR. Self-registering EXE servers respond to
# the -RegServer / -UnRegServer flags.
Write-Host '-- Registering OPC.SampleServer.1 via -RegServer'
& C:/server/opc_exe.exe -RegServer

try {
    Write-Host '-- Starting OPC Batch sample server (headless console)'
    & C:/server/opc_exe.exe
}
finally {
    Write-Host '-- Unregistering on shutdown'
    & C:/server/opc_exe.exe -UnRegServer
}
