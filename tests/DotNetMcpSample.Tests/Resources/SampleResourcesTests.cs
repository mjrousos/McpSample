using DotNetMcpSample.Resources;
using Xunit;

namespace DotNetMcpSample.Tests.Resources;

public class SampleResourcesTests
{
    [Fact]
    public void GetAppInfo_ReturnsJsonWithExpectedFields()
    {
        var resources = new SampleResources();
        var result = resources.GetAppInfo();

        Assert.Contains("DotNetMcpSample", result);
        Assert.Contains("framework", result);
    }

    [Fact]
    public void GetGreeting_ReturnsPersonalizedMessage()
    {
        var resources = new SampleResources();
        var result = resources.GetGreeting("Alice");

        Assert.Contains("Alice", result);
        Assert.Contains("Hello", result);
    }
}
