# MCP client integrations

This page collects MCP stdio configuration for each supported AI client. Install the server first with `dotnet tool install --global Opc.Classic.Mcp` — see [README.md](README.md) for the rest of the Opc.Classic.Mcp guide.

After saving the chosen client's config file, restart the client. The server name shown to the agent is `opc-classic`; the tool names use the `opcclassic.<spec>.<verb>` pattern.

## Claude Desktop

Create or update `%APPDATA%\Claude\claude_desktop_config.json` with:

```json
{
  "mcpServers": {
    "opc-classic": {
      "command": "opc-classic-mcp",
      "args": []
    }
  }
}
```

## Cursor

Create or update one of these files:

- Workspace: `.cursor\mcp.json`
- User: `~\.cursor\mcp.json`

with:

```json
{
  "mcpServers": {
    "opc-classic": {
      "command": "opc-classic-mcp"
    }
  }
}
```

## VS Code Copilot Chat

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

## GitHub Copilot CLI

Create or update the repository or CLI workspace file `.copilot\mcp.json` with:

```json
{
  "mcpServers": {
    "opc-classic": {
      "command": "opc-classic-mcp"
    }
  }
}
```

## Credentials and environment

No environment variable is required to launch the server. For real credentials, do not put literal passwords in this JSON. Prefer your client or operating-system secret store, then have the agent provide credentials to connect/logon tools only when needed. If you use the optional `appsettings.template` convention, set the referenced password variables such as `OPC_CLASSIC_PLANTHIST_PASSWORD` in the environment used to launch the MCP client.
