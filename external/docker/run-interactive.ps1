# SPDX-License-Identifier: MIT
# Copyright (c) 2026 Opc.Classic .NET Contributors
#
# Bring the fleet up (servers + an interactive c-client) and drop into a
# PowerShell session inside the c-client container for ad-hoc OPC method
# calls against the managed and native servers.
#
# Usage:
#   external/docker/run-interactive.ps1
# Inside the container:
#   & C:/client/opc-test.exe Opc.Classic.DaSample.1 opc-classic-managed
#   & C:/client/opc-test.exe OPC.SampleServer.1 opc-classic-c-server

[CmdletBinding()]
param([switch] $SkipBuild)

$ErrorActionPreference = 'Stop'
$root = Split-Path -Parent $PSCommandPath
$compose = "$root/docker-compose.test.yml"

# Ensure the network exists
if (-not (docker network ls --format '{{.Name}}' | Where-Object { $_ -eq 'opc-test-net' })) {
    docker network create --driver l2bridge --subnet 10.0.1.0/24 --gateway 10.0.1.1 opc-test-net
}

if (-not $SkipBuild) {
    docker compose --file $compose --profile interactive build
}

docker compose --file $compose up -d c-server managed-server
docker compose --file $compose run --rm --entrypoint powershell c-client
# When you exit the c-client shell, tear down the servers.
docker compose --file $compose down
