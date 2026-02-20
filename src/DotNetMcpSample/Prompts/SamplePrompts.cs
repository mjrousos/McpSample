using Microsoft.Extensions.AI;
using ModelContextProtocol.Server;
using System.ComponentModel;

namespace DotNetMcpSample.Prompts;

/// <summary>
/// Demonstrates MCP Prompts at three complexity levels:
/// simple (string), parameterized (string with args), and multi-turn (ChatMessage sequence).
/// </summary>
[McpServerPromptType]
public static class SamplePrompts
{
    [McpServerPrompt(Name = "summarize")]
    [Description("A prompt that asks the AI to summarize content")]
    public static string Summarize() =>
        "Please provide a concise summary of the following content:";

    [McpServerPrompt(Name = "code_review")]
    [Description("Generates a code review prompt for a specific language and focus areas")]
    public static string CodeReview(
        [Description("Programming language to review")] string language,
        [Description("Comma-separated focus areas (e.g. security, performance)")] string focusAreas) =>
        $"Review the following {language} code. Focus on: {focusAreas}. " +
        "Identify bugs, suggest improvements, and note any best-practice violations.";

    [McpServerPrompt(Name = "conversation_starter")]
    [Description("A multi-turn prompt that starts a conversation about a topic")]
    public static IEnumerable<ChatMessage> ConversationStarter(
        [Description("The topic to discuss")] string topic) =>
    [
        new(ChatRole.System, "You are a knowledgeable assistant who explains topics clearly and concisely."),
        new(ChatRole.User, $"I'd like to learn about {topic}. Can you give me a brief overview?")
    ];
}
