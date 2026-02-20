using ModelContextProtocol.Client;
using ModelContextProtocol.Protocol;
using Xunit;

namespace DotNetMcpSample.Tests.Integration;

public class StdioTransportTests : IAsyncLifetime
{
    private McpClient? _client;

    public Task InitializeAsync() => Task.CompletedTask;

    public async Task DisposeAsync()
    {
        if (_client is not null)
        {
            await _client.DisposeAsync();
        }
    }

    private async Task<McpClient> CreateClientAsync()
    {
        var projectPath = Path.GetFullPath(
            Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "..", "src", "DotNetMcpSample", "DotNetMcpSample.csproj"));

        var transport = new StdioClientTransport(new StdioClientTransportOptions
        {
            Name = "DotNetMcpSample-Test",
            Command = "dotnet",
            Arguments = ["run", "--project", projectPath, "--no-build", "--", "--transport", "stdio"]
        });

        _client = await McpClient.CreateAsync(transport);
        return _client;
    }

    [Fact]
    public async Task ListTools_ReturnsEchoTool()
    {
        var client = await CreateClientAsync();
        var tools = await client.ListToolsAsync();

        Assert.Contains(tools, t => t.Name == "echo");
    }

    [Fact]
    public async Task CallEchoTool_ReturnsExpectedResponse()
    {
        var client = await CreateClientAsync();
        var result = await client.CallToolAsync("echo", new Dictionary<string, object?> { ["message"] = "integration test" });

        Assert.NotNull(result);
        Assert.Contains("Echo: integration test", result.Content.OfType<TextContentBlock>().First().Text);
    }
}
