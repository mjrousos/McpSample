using DotNetMcpSample.Tools;
using Xunit;

namespace DotNetMcpSample.Tests.Tools;

public class StructuredOutputToolTests
{
    [Fact]
    public void AnalyzeText_ReturnsCorrectCounts()
    {
        var result = StructuredOutputTool.AnalyzeText("hello world");

        Assert.Equal(11, result.Characters);
        Assert.Equal(2, result.Words);
        Assert.Equal(1, result.Lines);
    }

    [Fact]
    public void AnalyzeText_WithMultipleLines_CountsLines()
    {
        var result = StructuredOutputTool.AnalyzeText("line one\nline two\nline three");

        Assert.Equal(3, result.Lines);
        Assert.Equal(6, result.Words);
    }

    [Fact]
    public void AnalyzeText_EmptyString_ReturnsZeroCounts()
    {
        var result = StructuredOutputTool.AnalyzeText("");

        Assert.Equal(0, result.Characters);
        Assert.Equal(0, result.Words);
        Assert.Equal(1, result.Lines); // Split on empty string yields one segment
    }
}
