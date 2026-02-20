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
