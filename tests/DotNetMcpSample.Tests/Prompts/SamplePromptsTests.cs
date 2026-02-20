using DotNetMcpSample.Prompts;
using Xunit;

namespace DotNetMcpSample.Tests.Prompts;

public class SamplePromptsTests
{
    [Fact]
    public void Summarize_ReturnsPromptString()
    {
        var result = SamplePrompts.Summarize();
        Assert.Contains("summary", result, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void CodeReview_IncludesLanguageAndFocusAreas()
    {
        var result = SamplePrompts.CodeReview("C#", "security, performance");

        Assert.Contains("C#", result);
        Assert.Contains("security, performance", result);
    }

    [Fact]
    public void ConversationStarter_ReturnsMultiTurnMessages()
    {
        var messages = SamplePrompts.ConversationStarter("quantum computing").ToList();

        Assert.Equal(2, messages.Count);
        Assert.Contains("quantum computing", messages[1].Text ?? "");
    }
}
