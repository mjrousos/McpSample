using DotNetMcpSample.Tools;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace DotNetMcpSample.Tests.Tools;

public class TimeInfoToolTests
{
    [Fact]
    public void GetCurrentTime_ReturnsUtcTimestamp()
    {
        var logger = Mock.Of<ILogger<TimeInfoTool>>();
        var tool = new TimeInfoTool(logger);

        var result = tool.GetCurrentTime();

        Assert.StartsWith("Current UTC time:", result);
        Assert.Contains(DateTimeOffset.UtcNow.Year.ToString(), result);
    }
}
