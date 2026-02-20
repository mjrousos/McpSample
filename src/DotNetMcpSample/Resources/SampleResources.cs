using System.Text.Json;
using ModelContextProtocol.Server;
using System.ComponentModel;

namespace DotNetMcpSample.Resources;

/// <summary>
/// Demonstrates MCP Resources — both static URIs and dynamic resource templates.
/// Resources expose read-only data that clients can browse and retrieve.
/// </summary>
[McpServerResourceType]
public class SampleResources
{
    [McpServerResource(
        UriTemplate = "config://app/info",
        Name = "Application Info",
        MimeType = "application/json")]
    [Description("Returns server metadata as JSON")]
    public string GetAppInfo()
    {
        return JsonSerializer.Serialize(new
        {
            name = "DotNetMcpSample",
            version = typeof(SampleResources).Assembly.GetName().Version?.ToString() ?? "0.1.0",
            framework = System.Runtime.InteropServices.RuntimeInformation.FrameworkDescription,
            timestamp = DateTimeOffset.UtcNow
        });
    }

    [McpServerResource(
        UriTemplate = "sample://greeting/{name}",
        Name = "Personalized Greeting")]
    [Description("Returns a personalized greeting for the given name")]
    public string GetGreeting(
        [Description("Name of the person to greet")] string name)
    {
        return $"Hello, {name}! Welcome to the DotNetMcpSample MCP server.";
    }
}
