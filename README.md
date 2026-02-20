# Echo MCP Server

A sample [Model Context Protocol (MCP)](https://modelcontextprotocol.io/) server implemented in C# using the [.NET MCP SDK](https://github.com/modelcontextprotocol/csharp-sdk). It exposes a simple `echo` tool and demonstrates both **stdio** and **HTTP** transports, making it a useful starting point for building your own MCP server.

## Quick Start

```bash
# Clone and build
git clone <repo-url>
dotnet build

# Run in stdio mode (default) — used by most local MCP clients
dotnet run --project src/ScryfallMCP

# Run in HTTP mode — for remote or browser-based clients
dotnet run --project src/ScryfallMCP -- --transport http
```

## Documentation

- **[MCP Quickstart Guide](docs/mcp-quickstart.md)** — How MCP works, how tools are defined, server startup, transport configuration, client setup, and how to add new tools.

## External Resources

- [Model Context Protocol Specification](https://modelcontextprotocol.io/)
- [C# MCP SDK on GitHub](https://github.com/modelcontextprotocol/csharp-sdk)
- [Build a Remote MCP Server in C# — .NET Blog](https://devblogs.microsoft.com/dotnet/build-a-remote-mcp-server-with-dotnet/)

## Connect a Client

### VS Code (GitHub Copilot)

A `.vscode/mcp.json` is already included. Open the workspace in VS Code and Copilot will discover the `echo-mcp` server automatically.

### Claude Desktop

Add to `claude_desktop_config.json`:

```json
{
  "mcpServers": {
    "echo-mcp": {
      "command": "dotnet",
      "args": ["run", "--project", "path/to/src/ScryfallMCP"]
    }
  }
}
```

## Running Tests

```bash
# All tests
dotnet test

# Unit tests only
dotnet test --filter "FullyQualifiedName~Tools"

# Integration tests only
dotnet test --filter "FullyQualifiedName~Integration"
```
