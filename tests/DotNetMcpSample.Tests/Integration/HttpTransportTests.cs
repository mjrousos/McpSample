using ModelContextProtocol.Client;
using ModelContextProtocol.Protocol;
using Xunit;

namespace ScryfallMCP.Tests.Integration;

public class HttpTransportTests : IAsyncLifetime
{
    private McpClient? _client;
    private System.Diagnostics.Process? _serverProcess;

    public Task InitializeAsync() => Task.CompletedTask;

    public async Task DisposeAsync()
    {
        if (_client is not null)
        {
            await _client.DisposeAsync();
        }

        if (_serverProcess is { HasExited: false })
        {
            _serverProcess.Kill(entireProcessTree: true);
            _serverProcess.Dispose();
        }
    }

    private async Task<McpClient> CreateClientAsync()
    {
        // Find a random available port
        var listener = new System.Net.Sockets.TcpListener(System.Net.IPAddress.Loopback, 0);
        listener.Start();
        var port = ((System.Net.IPEndPoint)listener.LocalEndpoint).Port;
        listener.Stop();

        var projectPath = Path.GetFullPath(
            Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "..", "src", "ScryfallMCP", "ScryfallMCP.csproj"));

        var url = $"http://localhost:{port}";
        _serverProcess = new System.Diagnostics.Process
        {
            StartInfo = new System.Diagnostics.ProcessStartInfo
            {
                FileName = "dotnet",
                ArgumentList = { "run", "--project", projectPath, "--no-build", "--", "--transport", "http", "--urls", url },
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            }
        };

        _serverProcess.Start();

        // Wait for the server to be ready
        using var httpClient = new HttpClient();
        var ready = false;
        for (var i = 0; i < 30; i++)
        {
            try
            {
                await Task.Delay(500);
                var response = await httpClient.PostAsync($"{url}/mcp", null);
                // MCP endpoint responds (even with error) means server is up
                ready = true;
                break;
            }
            catch
            {
                // Server not ready yet
            }
        }

        if (!ready)
        {
            _serverProcess.Kill(entireProcessTree: true);
            throw new InvalidOperationException("Server did not start in time");
        }

        var transport = new HttpClientTransport(new HttpClientTransportOptions
        {
            Endpoint = new Uri($"{url}/mcp")
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
        var result = await client.CallToolAsync("echo", new Dictionary<string, object?> { ["message"] = "http test" });

        Assert.NotNull(result);
        Assert.Contains("Echo: http test", result.Content.OfType<TextContentBlock>().First().Text);
    }
}
