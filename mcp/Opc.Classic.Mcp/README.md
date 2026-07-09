# Opc.Classic.Mcp

`Opc.Classic.Mcp` is the stdio Model Context Protocol server for Opc.Classic. It exposes the managed OPC Classic client stack as MCP tools so AI agents can discover servers, manage sessions, browse DA/AE/HDA models, read and write values, poll events and subscriptions, inspect Batch/Commands/Cpx metadata, manage DX configuration, use OPC Security, call XML-DA endpoints, and capture/decode OPC Classic DCOM traffic.

For the full user guide, architecture diagram, authentication notes, and client-specific setup snippets, see [docs/mcp/README.md](https://github.com/marcschier/opc-classic/blob/main/docs/mcp/README.md). For the current tool catalog, see `docs\mcp\tools.md`.

## Install from NuGet

```powershell
dotnet tool install --global Opc.Classic.Mcp
```

The installed command is:

```powershell
opc-classic-mcp
```

Use that command in MCP clients that support stdio servers:

```json
{
  "mcpServers": {
    "opc-classic": {
      "command": "opc-classic-mcp"
    }
  }
}
```

MCP clients that support NuGet package acquisition can launch the package directly with `dnx`:

```json
{
  "servers": {
    "opc-classic": {
      "type": "stdio",
      "command": "dnx",
      "args": [
        "Opc.Classic.Mcp",
        "--version",
        "<version>",
        "--yes"
      ]
    }
  }
}
```

## Run from source

```powershell
dotnet run --project mcp\Opc.Classic.Mcp
```

A local MCP client can launch the source tree directly:

```json
{
  "servers": {
    "opc-classic": {
      "type": "stdio",
      "command": "dotnet",
      "args": [
        "run",
        "--project",
        "mcp\\Opc.Classic.Mcp"
      ]
    }
  }
}
```

## Package

```powershell
dotnet pack mcp\Opc.Classic.Mcp\Opc.Classic.Mcp.csproj -c Release
```

The project is packaged as a .NET tool and MCP server package (`<PackAsTool>true</PackAsTool>`, `<PackageType>McpServer</PackageType>`, `<ToolCommandName>opc-classic-mcp</ToolCommandName>`).
