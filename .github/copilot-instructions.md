# Copilot Instructions for Scryfall-MCP

## Project Overview

This is a .NET MCP (Model Context Protocol) server that wraps the [Scryfall REST API](https://scryfall.com/docs/api) to expose Magic: The Gathering card data as MCP tools. It allows AI assistants to search for cards, retrieve card details, rulings, sets, and other MTG data through the Model Context Protocol.

## Target Framework & Language

- All projects target **.NET 10** (`<TargetFramework>net10.0</TargetFramework>`)
- Use the **latest C# language version** (`<LangVersion>latest</LangVersion>`)
- Prefer modern C# features: file-scoped namespaces, primary constructors, collection expressions, `required` properties, raw string literals, pattern matching

## NuGet Packages

### MCP SDK (prerelease — use `--prerelease` flag)

| Package | Purpose |
|---------|---------|
| `ModelContextProtocol` | Core SDK — tool attributes (`[McpServerToolType]`, `[McpServerTool]`), DI extensions (`AddMcpServer`), stdio transport (`WithStdioServerTransport`), and protocol types. Required by all projects. |
| `ModelContextProtocol.AspNetCore` | HTTP transport support — `WithHttpTransport()` and `MapMcp()` endpoint mapping for ASP.NET Core. Required only by the server project for HTTP mode. |
| `ModelContextProtocol.Core` | Minimal-dependency package with low-level client/server primitives. Typically not referenced directly — pulled in transitively by the above packages. |

### .NET / Microsoft Extensions

| Package | Purpose |
|---------|---------|
| `Microsoft.Extensions.Hosting` | Generic host (`Host.CreateApplicationBuilder`) for the stdio transport path. Provides DI, configuration, and logging infrastructure. |
| `Microsoft.Extensions.Http` | `IHttpClientFactory` registration (`AddHttpClient`) for making Scryfall API calls with proper lifetime management and `User-Agent` headers. |
| `Microsoft.Extensions.Logging.Console` | Console logging provider configured to write to stderr. |

### Testing

| Package | Purpose |
|---------|---------|
| `Microsoft.NET.Test.Sdk` | Test host infrastructure for running tests via `dotnet test`. |
| `xunit` / `xunit.runner.visualstudio` | Test framework and runner (or substitute `MSTest` / `NUnit` if preferred). |
| `Moq` | Mocking `HttpMessageHandler` / `IHttpClientFactory` for unit testing tool classes without hitting Scryfall. |

## Architecture

- **Solution format**: `.slnx` (new XML-based Visual Studio solution format)
- **MCP SDK**: Uses the official [C# MCP SDK](https://github.com/modelcontextprotocol/csharp-sdk) (see NuGet packages above)
- **Dual transport**: The server supports both **stdio** and **HTTP (Streamable HTTP)** transports, selectable at startup
  - **Stdio**: Uses `WithStdioServerTransport()` via `Host.CreateApplicationBuilder` — for local use as a subprocess (e.g., VS Code Copilot, Claude Desktop)
  - **HTTP**: Uses `WithHttpTransport()` via `WebApplication.CreateBuilder` with `MapMcp()` endpoint — for remote/production deployments
- **Tool discovery**: Attribute-based using `[McpServerToolType]` on tool classes and `[McpServerTool]` on tool methods, auto-registered via `WithToolsFromAssembly()`
- **DI**: Uses `Microsoft.Extensions.Hosting` and standard .NET dependency injection for `HttpClient`, logging, and services

## Build & Test

```shell
# Restore and build
dotnet build

# Run all tests
dotnet test

# Run a specific test class or method
dotnet test --filter "FullyQualifiedName~ClassName.MethodName"

# Run the server (stdio mode is default)
dotnet run --project src/ScryfallMCP

# Run the server in HTTP mode
dotnet run --project src/ScryfallMCP -- --transport http
```

## MCP Best Practices (per Microsoft/.NET guidelines)

### Testing Requirements

- All new features must include unit tests validating their functionality
- Test tool classes by mocking `HttpMessageHandler` with Moq to avoid real Scryfall API calls
- Place tests in a corresponding test project under `tests/`

### Tool Design

- Each logical group of tools lives in its own class marked with `[McpServerToolType]`
- Individual tool methods use `[McpServerTool]` and `[Description("...")]` attributes — the SDK auto-generates JSON schemas from these
- Tool parameters use `[Description("...")]` to document each parameter for the calling AI agent
- Use `CancellationToken` on all async tool methods
- Throw `McpException` for user-facing errors from tools
- Keep tools modular and focused — don't create monolithic "mega-tools" that do too many things
- Mark read-only tools with `ReadOnly = true` and idempotent tools with `Idempotent = true` on the `[McpServerTool]` attribute

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

## Scryfall API Integration

- Base URL: `https://api.scryfall.com`
- No authentication required
- Scryfall asks that clients send a reasonable `User-Agent` header and limit to ~10 requests/second
- Use `IHttpClientFactory` (registered via `AddHttpClient`) rather than raw `HttpClient`
- Key endpoints: `/cards/search`, `/cards/named`, `/cards/{id}`, `/cards/{id}/rulings`, `/sets`, `/catalog/{category}`

## Project Structure

```
src/
  ScryfallMCP/           # MCP server host + tool definitions
tests/
  ScryfallMCP.Tests/     # Unit and integration tests
```
