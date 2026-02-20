using DotNetMcpSample.Tools;
using Xunit;

namespace DotNetMcpSample.Tests.Tools;

public class EchoToolTests
{
    [Fact]
    public void Echo_ReturnsFormattedMessage()
    {
        var result = EchoTool.Echo("hello world");
        Assert.Equal("Echo: hello world", result);
    }

    [Fact]
    public void Echo_WithEmptyString_ReturnsEchoPrefix()
    {
        var result = EchoTool.Echo("");
        Assert.Equal("Echo: ", result);
    }

    [Fact]
    public void Echo_WithSpecialCharacters_ReturnsVerbatim()
    {
        var result = EchoTool.Echo("héllo <world> & \"friends\"");
        Assert.Equal("Echo: héllo <world> & \"friends\"", result);
    }
}
