# Samba WINREG smoke fixture

Linux-container fixture for the cap-h7 WINREG end-to-end smoke. It exposes SMB on host port `445`, requires SMB2+ with signing, and enables Samba's `winreg` DCE/RPC endpoint on `\\PIPE\\winreg`.

Credentials are fixed for the disposable test fixture:

- Domain/workgroup: `TESTDOMAIN`
- User: `opcuser`
- Password: `opcpass`

## Run locally on a Linux Docker host

```sh
docker compose -f external/docker/samba/docker-compose.yml up -d --build
OPC_CLASSIC_INTEGRATION_SAMBA=1 \
OPC_CLASSIC_SAMBA_HOST=127.0.0.1 \
OPC_CLASSIC_SAMBA_USER=opcuser \
OPC_CLASSIC_SAMBA_PASSWORD=opcpass \
OPC_CLASSIC_SAMBA_DOMAIN=TESTDOMAIN \
  dotnet test tests/Opc.Classic.Integration.Tests -- --treenode-filter "/*/*/WinRegSambaSmokeTests/*"
docker compose -f external/docker/samba/docker-compose.yml down -v
```

Without `OPC_CLASSIC_INTEGRATION_SAMBA=1`, the test soft-skips before connecting.
