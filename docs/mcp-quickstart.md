# .NET MCP Server Quickstart

## What is MCP?

The [Model Context Protocol](https://modelcontextprotocol.io/) is an open standard that defines how AI assistants (called **clients**) communicate with external services (called **servers**) to discover and invoke **tools**. Think of it as a universal plugin interface for AI: any compliant client (VS Code Copilot, Claude Desktop, etc.) can connect to any compliant server and immediately use the tools it exposes.

The protocol uses [JSON-RPC 2.0](https://www.jsonrpc.org/) messages exchanged over a **transport** — either standard I/O (stdio) or HTTP (Streamable HTTP).

### Key Concepts

| Concept | Description |
|---------|-------------|
| **Server** | A process that exposes capabilities (tools, resources, prompts) to AI clients. This project is an MCP server. |
| **Client** | An AI host (e.g., VS Code Copilot) that connects to servers, discovers tools, and calls them on behalf of the user. |
| **Tool** | A function the server exposes. The client sees its name, description, and parameter schema, then calls it when the AI decides it's relevant. |
| **Transport** | How messages flow between client and server. MCP defines **stdio** (subprocess communication) and **HTTP** (network communication) transports. |

## The C# MCP SDK

This project uses the official [C# MCP SDK](https://github.com/modelcontextprotocol/csharp-sdk), distributed as prerelease NuGet packages:

| Package | Purpose |
|---------|---------|
| [`ModelContextProtocol`](https://www.nuget.org/packages/ModelContextProtocol) | Core SDK — server/client abstractions, tool attributes, DI extensions, stdio transport. |
| [`ModelContextProtocol.AspNetCore`](https://www.nuget.org/packages/ModelContextProtocol.AspNetCore) | ASP.NET Core integration — HTTP transport via `WithHttpTransport()` and `MapMcp()`. |

These are referenced in the server project's [DotNetMcpSample.csproj](../src/DotNetMcpSample/DotNetMcpSample.csproj):

```xml
<PackageReference Include="ModelContextProtocol" Version="*-*" />
<PackageReference Include="ModelContextProtocol.AspNetCore" Version="*-*" />
```

> **Note:** The `*-*` version specifier pulls the latest prerelease. Pin to a specific version for production use.

## Defining Tools

Tools are the primary way an MCP server exposes functionality. The SDK uses an **attribute-based** approach for tool discovery.

### Tool Class and Method Attributes

Look at [`src/DotNetMcpSample/Tools/EchoTool.cs`](../src/DotNetMcpSample/Tools/EchoTool.cs):

```csharp
using ModelContextProtocol.Server;
using System.ComponentModel;

namespace DotNetMcpSample.Tools;

[McpServerToolType]
public static class EchoTool
{
    [McpServerTool(Name = "echo", ReadOnly = true, Idempotent = true)]
    [Description("Echoes the input message back. Useful for testing connectivity.")]
    public static string Echo(
        [Description("The message to echo back")] string message) =>
        $"Echo: {message}";
}
```

Here's what each piece does:

| Element | Purpose |
|---------|---------|
| `[McpServerToolType]` | Marks the class as containing MCP tools. The SDK scans for these during registration. |
| `[McpServerTool]` | Marks a method as an individual tool. The `Name` property sets the tool name that clients see. |
| `ReadOnly = true` | Hints to the client that this tool does not modify any state. |
| `Idempotent = true` | Hints to the client that calling this tool multiple times with the same input produces the same result. |
| `[Description("...")]` | Provides a human-readable description. The SDK uses this to generate the JSON schema that clients read to understand what the tool does and what parameters it accepts. |
| Parameter attributes | Each parameter gets a `[Description]` so the AI knows what to pass. The SDK automatically generates the JSON schema from the method signature (parameter names, types, and descriptions). |

### How Tool Discovery Works

The SDK automatically discovers all tools at startup via `WithToolsFromAssembly()` (explained below). It:

1. Scans the assembly for classes marked `[McpServerToolType]`
2. Finds methods marked `[McpServerTool]` in those classes
3. Generates a JSON schema for each tool's parameters from the method signature
4. Responds to the client's `tools/list` request with all discovered tools

No manual tool registration is needed — just add the attributes and the SDK handles the rest.

## Server Startup and Transport Configuration

The server's entry point is [`src/DotNetMcpSample/Program.cs`](../src/DotNetMcpSample/Program.cs). It supports two transports, selectable via a `--transport` command-line argument.

### Stdio Transport

```csharp
static async Task RunStdioAsync(string[] args)
{
    var builder = Host.CreateApplicationBuilder(args);
    builder.Logging.AddConsole(consoleLogOptions =>
    {
        consoleLogOptions.LogToStandardErrorThreshold = LogLevel.Trace;
    });

    builder.Services
        .AddMcpServer(options =>
        {
            options.ServerInfo = new()
            {
                Name = "DotNetMcpSample",
                Title = "DotNet MCP Sample Server",
                Description = "A .NET MCP reference server demonstrating tools, resources, prompts, and more.",
                Version = GetVersion()
            };
        })
        .WithStdioServerTransport()
        .WithToolsFromAssembly();

    await builder.Build().RunAsync();
}
```

Key points:

- **`Host.CreateApplicationBuilder`** — Uses the .NET Generic Host for DI, configuration, and lifecycle management.
- **`AddMcpServer`** — Registers the MCP server in the DI container and configures server metadata (name, description, version). This metadata is sent to clients during the `initialize` handshake.
- **`WithStdioServerTransport()`** — Configures the server to communicate over standard input/output. The client launches this server as a subprocess and exchanges JSON-RPC messages via stdin/stdout.
- **`WithToolsFromAssembly()`** — Scans the current assembly for `[McpServerToolType]` classes and registers all `[McpServerTool]` methods.
- **Logging to stderr** — Critical for stdio mode! Stdout is reserved for MCP protocol messages, so all logging must go to stderr. The `LogToStandardErrorThreshold = LogLevel.Trace` setting redirects console logging accordingly.

### HTTP Transport (Streamable HTTP)

```csharp
static async Task RunHttpAsync(string[] args)
{
    var builder = WebApplication.CreateBuilder(args);
    builder.Logging.AddConsole();

    builder.Services
        .AddMcpServer(options =>
        {
            options.ServerInfo = new()
            {
                Name = "DotNetMcpSample",
                Title = "DotNet MCP Sample Server",
                Description = "A .NET MCP reference server demonstrating tools, resources, prompts, and more.",
                Version = GetVersion()
            };
        })
        .WithHttpTransport()
        .WithToolsFromAssembly();

    var app = builder.Build();
    app.MapMcp("/mcp");
    await app.RunAsync();
}
```

Key points:

- **`WebApplication.CreateBuilder`** — Uses ASP.NET Core instead of the Generic Host since HTTP transport needs a web server.
- **`WithHttpTransport()`** — Registers the MCP HTTP transport services. Requires the `ModelContextProtocol.AspNetCore` NuGet package.
- **`MapMcp("/mcp")`** — Maps the MCP endpoint to the `/mcp` URL path. Clients send JSON-RPC requests here via HTTP POST, and the server can stream responses using Server-Sent Events (SSE).

### Choosing a Transport

| Transport | Use Case | How the Client Connects |
|-----------|----------|-------------------------|
| **Stdio** | Local AI tools (VS Code Copilot, Claude Desktop) that launch the server as a subprocess. | Client spawns the process and pipes stdin/stdout. |
| **HTTP** | Remote deployments, shared servers, or web-based clients. | Client sends HTTP requests to the server's URL. |

## Running the Server

```bash
# Stdio mode (default) — used by most local MCP clients
dotnet run --project src/DotNetMcpSample

# HTTP mode — for remote or browser-based clients
dotnet run --project src/DotNetMcpSample -- --transport http
```

## Configuring a Client to Connect

### VS Code (GitHub Copilot)

Add the server to your VS Code MCP settings (`.vscode/mcp.json` or user settings):

```json
{
  "servers": {
    "dotnet-mcp-sample": {
      "command": "dotnet",
      "args": ["run", "--project", "path/to/src/DotNetMcpSample"]
    }
  }
}
```

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

## Testing

The project includes both unit tests and integration tests.

### Unit Tests

Tool methods are plain C# methods, so they can be tested directly without the MCP infrastructure:

```csharp
[Fact]
public void Echo_ReturnsFormattedMessage()
{
    var result = EchoTool.Echo("hello world");
    Assert.Equal("Echo: hello world", result);
}
```

### Integration Tests

The integration tests start the actual MCP server and connect to it using the SDK's client classes. See [`tests/DotNetMcpSample.Tests/Integration/`](../tests/DotNetMcpSample.Tests/Integration/) for examples using both transports:

- **`StdioTransportTests`** — Creates a `StdioClientTransport` that launches the server as a subprocess.
- **`HttpTransportTests`** — Starts the server in HTTP mode and connects with an `HttpClientTransport`.

Both test suites verify tool discovery (`ListToolsAsync`) and tool invocation (`CallToolAsync`).

```bash
# Run all tests
dotnet test

# Run only unit tests
dotnet test --filter "FullyQualifiedName~Tools"

# Run only integration tests
dotnet test --filter "FullyQualifiedName~Integration"
```

## Adding a New Tool

To add a new tool to this server:

1. **Create a tool class** in `src/DotNetMcpSample/Tools/`:

    ```csharp
    using ModelContextProtocol.Server;
    using System.ComponentModel;

    namespace DotNetMcpSample.Tools;

    [McpServerToolType]
    public static class MyTool
    {
        [McpServerTool(Name = "my-tool", ReadOnly = true)]
        [Description("Describe what this tool does for the AI agent.")]
        public static string DoSomething(
            [Description("Describe this parameter")] string input) =>
            $"Result: {input}";
    }
    ```

2. **That's it for registration** — `WithToolsFromAssembly()` picks it up automatically.

3. **Add unit tests** in `tests/DotNetMcpSample.Tests/Tools/`.

4. **Rebuild and reconnect** your MCP client to see the new tool.

### Tips for Tool Design

- **Write clear descriptions** — The AI reads `[Description]` attributes to decide when and how to use your tool. Be specific.
- **Keep tools focused** — One tool should do one thing well. Don't create mega-tools with many optional parameters.
- **Use `CancellationToken`** — Add it as the last parameter on async tool methods so the client can cancel long-running operations.
- **Mark behavioral hints** — Use `ReadOnly = true` for tools that don't modify state, and `Idempotent = true` for tools that are safe to retry.
- **Validate inputs** — Don't trust that the AI will always send valid data. Throw `McpException` for user-facing errors.

## Further Reading

- [Model Context Protocol Specification](https://modelcontextprotocol.io/)
- [C# MCP SDK on GitHub](https://github.com/modelcontextprotocol/csharp-sdk)
- [Build a Remote MCP Server in C# — .NET Blog](https://devblogs.microsoft.com/dotnet/build-a-remote-mcp-server-with-dotnet/)
