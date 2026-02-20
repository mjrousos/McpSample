var transport = args.FirstOrDefault(a => a == "--transport" || a == "-t") is not null
    ? args[Array.IndexOf(args, args.First(a => a == "--transport" || a == "-t")) + 1]
    : "stdio";

if (transport.Equals("http", StringComparison.OrdinalIgnoreCase))
{
    await RunHttpAsync(args);
}
else
{
    await RunStdioAsync(args);
}

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
                Name = "Echo-MCP",
                Title = "Echo MCP Server",
                Description = "A simple MCP server that echoes back received messages. Can be used for testing that MCP clients are working correctly.",
                Version = GetVersion()
            };
        })
        .WithStdioServerTransport()
        .WithToolsFromAssembly()
        .WithPromptsFromAssembly()
        .WithResourcesFromAssembly();

    var app = builder.Build();
    var logger = app.Services.GetRequiredService<ILoggerFactory>().CreateLogger("EchoMCP");
    logger.LogInformation("Echo MCP server started in stdio mode. Connect by configuring your MCP client to launch this process.");
    await app.RunAsync();
}

static async Task RunHttpAsync(string[] args)
{
    var builder = WebApplication.CreateBuilder(args);
    builder.Logging.AddConsole();

    builder.Services
        .AddMcpServer(options =>
        {
            options.ServerInfo = new()
            {
                Name = "Echo-MCP",
                Title = "Echo MCP Server",
                Description = "A simple MCP server that echoes back received messages. Can be used for testing that MCP clients are working correctly.",
                Version = GetVersion()
            };
        })
        .WithHttpTransport()
        .WithToolsFromAssembly()
        .WithPromptsFromAssembly()
        .WithResourcesFromAssembly();

    var app = builder.Build();
    app.MapMcp("/mcp");

    app.Lifetime.ApplicationStarted.Register(() =>
    {
        var logger = app.Services.GetRequiredService<ILoggerFactory>().CreateLogger("EchoMCP");
        var addresses = string.Join(", ", app.Urls.Select(u => $"{u}/mcp"));
        logger.LogInformation("Echo MCP server started in HTTP mode. Connect your MCP client to: {Addresses}", addresses);
    });

    await app.RunAsync();
}

static string GetVersion() =>
    typeof(Program).Assembly.GetName().Version?.ToString() ?? "0.1.0";
