# VS Code Copilot Chat integration

Install the MCP server first:

```powershell
dotnet tool install --global Opc.Classic.Mcp
```

Create or update the workspace file `.vscode\mcp.json` with:

```json
{
  "servers": {
    "opc-classic": {
      "type": "stdio",
      "command": "opc-classic-mcp"
    }
  }
}
```

Restart the client after saving the file. The server name shown to the agent is `opc-classic`; the tool names use the `opcclassic.<spec>.<verb>` pattern.

## Credentials and environment

No environment variable is required to launch the server. For real credentials, do not put literal passwords in this JSON. Prefer your client or operating-system secret store, then have the agent provide credentials to connect/logon tools only when needed. If you use the optional `mcp\Opc.Classic.Mcp\appsettings.template.json` convention, set the referenced password variables such as `OPC_CLASSIC_PLANTHIST_PASSWORD` in the environment used to launch the MCP client.

See the full guide in [../README.md](../README.md).
