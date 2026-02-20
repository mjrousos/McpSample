# Copilot Instructions for DotNetMcpSample

## Project Overview

This is a .NET 10 reference implementation of an MCP (Model Context Protocol) server using the official [C# MCP SDK](https://github.com/modelcontextprotocol/csharp-sdk). It demonstrates key MCP patterns — tools, resources, prompts, structured output, dependency injection, and dual transport (stdio + HTTP). It serves as a practical starting point for building production MCP servers.

## Target Framework & Language

- All projects target **.NET 10** (`<TargetFramework>net10.0</TargetFramework>`)
- Use the **latest C# language version** (`<LangVersion>latest</LangVersion>`)
- Prefer modern C# features: file-scoped namespaces, primary constructors, collection expressions, `required` properties, raw string literals, pattern matching

## NuGet Packages

### MCP SDK (prerelease — use `--prerelease` flag)

| Package | Purpose |
|---------|---------|
| `ModelContextProtocol` | Core SDK — tool/resource/prompt attributes, DI extensions (`AddMcpServer`), stdio transport (`WithStdioServerTransport`), and protocol types. Required by all projects. |
| `ModelContextProtocol.AspNetCore` | HTTP transport support — `WithHttpTransport()` and `MapMcp()` endpoint mapping for ASP.NET Core. Required only by the server project for HTTP mode. |

### .NET / Microsoft Extensions

| Package | Purpose |
|---------|---------|
| `Microsoft.Extensions.Hosting` | Generic host (`Host.CreateApplicationBuilder`) for the stdio transport path. Provides DI, configuration, and logging infrastructure. |
| `Microsoft.Extensions.Logging.Console` | Console logging provider configured to write to stderr. |

### Testing

| Package | Purpose |
|---------|---------|
| `Microsoft.NET.Test.Sdk` | Test host infrastructure for running tests via `dotnet test`. |
| `xunit` / `xunit.runner.visualstudio` | Test framework and runner. |
| `Moq` | Mocking dependencies (e.g., `ILogger<T>`) for unit testing tool classes. |

## Architecture

- **Solution format**: `.slnx` (new XML-based Visual Studio solution format)
- **MCP SDK**: Uses the official [C# MCP SDK](https://github.com/modelcontextprotocol/csharp-sdk) (see NuGet packages above)
- **Dual transport**: The server supports both **stdio** and **HTTP (Streamable HTTP)** transports, selectable at startup via `--transport stdio|http`
  - **Stdio**: Uses `WithStdioServerTransport()` via `Host.CreateApplicationBuilder` — for local use as a subprocess (e.g., VS Code Copilot, Claude Desktop)
  - **HTTP**: Uses `WithHttpTransport()` via `WebApplication.CreateBuilder` with `MapMcp()` endpoint — for remote/production deployments
- **Capability discovery**: Attribute-based using assembly scanning:
  - Tools: `[McpServerToolType]` + `[McpServerTool]`, registered via `WithToolsFromAssembly()`
  - Resources: `[McpServerResourceType]` + `[McpServerResource]`, registered via `WithResourcesFromAssembly()`
  - Prompts: `[McpServerPromptType]` + `[McpServerPrompt]`, registered via `WithPromptsFromAssembly()`
- **DI**: Uses `Microsoft.Extensions.Hosting` and standard .NET dependency injection for logging, services, and non-static tool classes

## Build & Test

```shell
# Restore and build
dotnet build

# Run all tests
dotnet test

# Run a specific test class or method
dotnet test --filter "FullyQualifiedName~ClassName.MethodName"

# Run the server (stdio mode is default)
dotnet run --project src/DotNetMcpSample

# Run the server in HTTP mode
dotnet run --project src/DotNetMcpSample -- --transport http
```

## MCP Best Practices (per Microsoft/.NET guidelines)

### Testing Requirements

- All new features must include unit tests validating their functionality
- Test tool classes by mocking dependencies (e.g., `ILogger<T>`) with Moq
- Place tests in a corresponding test project under `tests/`

### Tool Design

- Each logical group of tools lives in its own class marked with `[McpServerToolType]`
- Individual tool methods use `[McpServerTool]` and `[Description("...")]` attributes — the SDK auto-generates JSON schemas from these
- Tool parameters use `[Description("...")]` to document each parameter for the calling AI agent
- Use `CancellationToken` on all async tool methods
- Throw `McpException` for user-facing errors from tools
- Keep tools modular and focused — don't create monolithic "mega-tools" that do too many things
- Mark read-only tools with `ReadOnly = true` and idempotent tools with `Idempotent = true` on the `[McpServerTool]` attribute
- For rich output, use `[McpServerTool(UseStructuredContent = true)]` and return `CallToolResult` with multiple content blocks

### Resource Design

- Use `[McpServerResourceType]` on classes and `[McpServerResource]` on methods
- Static resources use fixed URIs (e.g., `config://app/info`)
- Dynamic resources use URI templates with parameters (e.g., `sample://greeting/{name}`)

### Prompt Design

- Use `[McpServerPromptType]` on classes and `[McpServerPrompt]` on methods
- Simple prompts return `string`; multi-turn prompts return `IEnumerable<ChatMessage>` (from `Microsoft.Extensions.AI`)
- Use `[Description]` on prompt parameters for documentation

### Dependency Injection in Tools

- Non-static tool classes can accept services via primary constructors
- The MCP SDK resolves tools through DI, enabling `ILogger<T>`, `IHttpClientFactory`, etc.

### Security & Input Validation

- Validate and sanitize all incoming tool parameters (schema validation + boundary checks)
- For HTTP transport: use authentication (JWT/OAuth/API keys) and CORS restricted to trusted origins
- Follow least-privilege: only expose tools and data necessary for the use case
- Be aware of prompt injection risks — never trust tool inputs as safe
- Externalize secrets and configuration via environment variables or `IOptions<T>`, never hardcode

### Logging & Observability

- All logging goes to **stderr** (not stdout) — stdout is reserved for the MCP JSON-RPC stdio transport
- Configure via: `consoleLogOptions.LogToStandardErrorThreshold = LogLevel.Trace`
- Use structured logging with `ILogger<T>` throughout
- Log tool invocations for auditing and debugging

### Packaging & Distribution

- Support Native AOT (`--publishAot`) and self-contained publishing for portability
- MCP servers can be distributed as NuGet tool packages with a `server.json` manifest for discoverability
- Include a `server.json` file defining transport type, environment variables, and package arguments

## Project Structure

```
src/
  DotNetMcpSample/
    Program.cs                     # Entry point — dual transport selection
    Tools/                         # MCP tool classes
    Resources/                     # MCP resource classes
    Prompts/                       # MCP prompt classes
tests/
  DotNetMcpSample.Tests/
    Tools/                         # Unit tests for tools
    Resources/                     # Unit tests for resources
    Prompts/                       # Unit tests for prompts
    Integration/                   # Stdio and HTTP transport integration tests
docs/
  mcp-quickstart.md               # Comprehensive MCP walkthrough
```
