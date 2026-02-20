using DotNetMcpSample.Tools;
using ModelContextProtocol.Protocol;
using Xunit;

namespace DotNetMcpSample.Tests.Tools;

public class StructuredOutputToolTests
{
    [Fact]
    public void AnalyzeText_ReturnsCorrectCounts()
    {
        var result = StructuredOutputTool.AnalyzeText("hello world");

        Assert.Equal(2, result.Content.Count);
        var stats = result.Content.OfType<TextContentBlock>().First().Text;
        Assert.Contains("Characters: 11", stats);
        Assert.Contains("Words: 2", stats);
    }

    [Fact]
    public void AnalyzeText_WithMultipleLines_CountsLines()
    {
        var result = StructuredOutputTool.AnalyzeText("line one\nline two\nline three");
        var stats = result.Content.OfType<TextContentBlock>().First().Text;

        Assert.Contains("Lines: 3", stats);
        Assert.Contains("Words: 6", stats);
    }

    [Fact]
    public void AnalyzeText_EmptyString_ReturnsZeroCounts()
    {
        var result = StructuredOutputTool.AnalyzeText("");
        var stats = result.Content.OfType<TextContentBlock>().First().Text;

        Assert.Contains("Characters: 0", stats);
        Assert.Contains("Words: 0", stats);
    }
}
