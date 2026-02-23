# DotNetMcpSample

A .NET 10 reference implementation of a [Model Context Protocol (MCP)](https://modelcontextprotocol.io/) server using the official [C# MCP SDK](https://github.com/modelcontextprotocol/csharp-sdk). This project demonstrates key MCP patterns — tools, resources, prompts, structured output, dependency injection, and dual transport — making it a practical starting point for building your own MCP server.

## Quick Start

```bash
# Clone and build
git clone https://github.com/mjrousos/McpSample.git
cd McpSample
dotnet build

# Run in stdio mode (default) — used by most local MCP clients
dotnet run --project src/DotNetMcpSample

# Run in HTTP mode — for remote or browser-based clients
dotnet run --project src/DotNetMcpSample -- --transport http

# Run all tests
dotnet test
```

## Patterns Demonstrated

| Pattern | Source File | Description |
|---------|-----------|-------------|
| **Basic Tool** | [`Tools/EchoTool.cs`](src/DotNetMcpSample/Tools/EchoTool.cs) | Static tool with `[McpServerToolType]` / `[McpServerTool]` attribute-based discovery |
| **Structured Output** | [`Tools/StructuredOutputTool.cs`](src/DotNetMcpSample/Tools/StructuredOutputTool.cs) | Returns `CallToolResult` with multiple `TextContentBlock` entries |
| **DI-Injected Tool** | [`Tools/TimeInfoTool.cs`](src/DotNetMcpSample/Tools/TimeInfoTool.cs) | Non-static tool class with `ILogger<T>` injected via primary constructor |
| **Static Resource** | [`Resources/SampleResources.cs`](src/DotNetMcpSample/Resources/SampleResources.cs) | Fixed-URI resource (`config://app/info`) returning JSON metadata |
| **Resource Template** | [`Resources/SampleResources.cs`](src/DotNetMcpSample/Resources/SampleResources.cs) | Parameterized resource (`sample://greeting/{name}`) |
| **Simple Prompt** | [`Prompts/SamplePrompts.cs`](src/DotNetMcpSample/Prompts/SamplePrompts.cs) | Returns a plain string |
| **Parameterized Prompt** | [`Prompts/SamplePrompts.cs`](src/DotNetMcpSample/Prompts/SamplePrompts.cs) | Accepts arguments and returns a formatted string |
| **Multi-turn Prompt** | [`Prompts/SamplePrompts.cs`](src/DotNetMcpSample/Prompts/SamplePrompts.cs) | Returns `IEnumerable<ChatMessage>` with system + user messages |
| **Dual Transport** | [`Program.cs`](src/DotNetMcpSample/Program.cs) | Stdio (`WithStdioServerTransport`) and HTTP (`WithHttpTransport` + `MapMcp`) |

## NuGet Packages

| Package | Purpose |
|---------|---------|
| [`ModelContextProtocol`](https://www.nuget.org/packages/ModelContextProtocol) | Core MCP SDK — tool/resource/prompt attributes, DI extensions, stdio transport |
| [`ModelContextProtocol.AspNetCore`](https://www.nuget.org/packages/ModelContextProtocol.AspNetCore) | HTTP transport support — `WithHttpTransport()` and `MapMcp()` for ASP.NET Core |
| `Microsoft.Extensions.Hosting` | Generic host for the stdio transport path (DI, config, logging) |
| `Microsoft.Extensions.Logging.Console` | Console logging to stderr (stdout is reserved for MCP JSON-RPC in stdio mode) |

## Project Structure

```
src/
  DotNetMcpSample/
    Program.cs                     # Entry point — dual transport selection
    Tools/
      EchoTool.cs                  # Basic tool
      StructuredOutputTool.cs      # Rich multi-part output
      TimeInfoTool.cs              # DI-injected tool
    Resources/
      SampleResources.cs           # Static + templated resources
    Prompts/
      SamplePrompts.cs             # Simple, parameterized, multi-turn prompts
tests/
  DotNetMcpSample.Tests/
    Tools/                         # Unit tests for each tool
    Resources/                     # Unit tests for resources
    Prompts/                       # Unit tests for prompts
    Integration/                   # Stdio and HTTP transport integration tests
docs/
  mcp-quickstart.md               # Comprehensive MCP walkthrough
```

## Connect a Client

### VS Code (GitHub Copilot)

A `.vscode/mcp.json` is included. Open the workspace in VS Code and Copilot will discover the server automatically.

### Claude Desktop

Add to `claude_desktop_config.json`:

```json
{
  "mcpServers": {
    "dotnet-mcp-sample": {
      "command": "dotnet",
      "args": ["run", "--project", "path/to/src/DotNetMcpSample"]
    }
  }
}
```

## Running Tests

```bash
# All tests (unit + integration)
dotnet test

# Unit tests only
dotnet test --filter "FullyQualifiedName~Tools|FullyQualifiedName~Resources|FullyQualifiedName~Prompts"

# Integration tests only
dotnet test --filter "FullyQualifiedName~Integration"
```

## Further Reading

- [Model Context Protocol Specification](https://modelcontextprotocol.io/)
- [C# MCP SDK — GitHub](https://github.com/modelcontextprotocol/csharp-sdk)
- [Build an MCP Server in .NET — Microsoft Learn](https://learn.microsoft.com/dotnet/ai/quickstarts/build-mcp-server)
- [Get Started with MCP in .NET — Microsoft Learn](https://learn.microsoft.com/dotnet/ai/get-started-mcp)
- [MCP Quickstart Walkthrough](docs/mcp-quickstart.md) (in this repo)
