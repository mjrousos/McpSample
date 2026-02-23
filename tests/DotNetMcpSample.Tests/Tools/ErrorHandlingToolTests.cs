using DotNetMcpSample.Tools;
using ModelContextProtocol;
using Xunit;

namespace DotNetMcpSample.Tests.Tools;

public class ErrorHandlingToolTests
{
    // --- Divide: McpException path ---

    [Fact]
    public void Divide_ReturnsResult_WhenDivisorIsNonZero()
    {
        var result = ErrorHandlingTool.Divide(10, 4);
        Assert.Equal("Result: 2.5", result);
    }

    [Fact]
    public void Divide_ThrowsMcpException_WhenDivisorIsZero()
    {
        var ex = Assert.Throws<McpException>(() => ErrorHandlingTool.Divide(5, 0));
        Assert.Contains("zero", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    // --- SafeParseInt: CallToolResult with IsError ---

    [Fact]
    public void SafeParseInt_ReturnsSuccessResult_ForValidInteger()
    {
        var result = ErrorHandlingTool.SafeParseInt("42");

        Assert.True(result.IsError != true); // IsError is null (unset) on success
        Assert.Single(result.Content);
        Assert.Contains("42", result.Content[0].ToString());
    }

    [Fact]
    public void SafeParseInt_ReturnsErrorResult_ForInvalidInput()
    {
        var result = ErrorHandlingTool.SafeParseInt("not-a-number");

        Assert.True(result.IsError);
        Assert.Single(result.Content);
        Assert.Contains("not-a-number", result.Content[0].ToString());
    }

    [Fact]
    public void SafeParseInt_ReturnsSuccessResult_ForNegativeInteger()
    {
        var result = ErrorHandlingTool.SafeParseInt("-7");

        Assert.True(result.IsError != true); // IsError is null (unset) on success
        Assert.Single(result.Content);
        Assert.Contains("-7", result.Content[0].ToString());
    }
}
