using Microsoft.Extensions.Logging;
using ModelContextProtocol.Server;
using System.ComponentModel;

namespace DotNetMcpSample.Tools;

/// <summary>
/// Demonstrates dependency injection in MCP tools.
/// Non-static tool class — the MCP SDK instantiates it via DI,
/// so constructor-injected services (like ILogger) are available.
/// </summary>
[McpServerToolType]
public class TimeInfoTool(ILogger<TimeInfoTool> logger)
{
    [McpServerTool(Name = "get_time", ReadOnly = true, Idempotent = false)]
    [Description("Returns the current UTC date and time. Demonstrates DI-injected tools.")]
    public string GetCurrentTime()
    {
        var now = DateTimeOffset.UtcNow;
        logger.LogInformation("get_time tool invoked at {Timestamp}", now);
        return $"Current UTC time: {now:yyyy-MM-dd HH:mm:ss.fff zzz}";
    }
}
