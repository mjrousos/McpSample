using ModelContextProtocol.Protocol;
using ModelContextProtocol.Server;
using System.ComponentModel;

namespace DotNetMcpSample.Tools;

/// <summary>
/// Demonstrates returning structured, multi-part content from an MCP tool
/// using CallToolResult with multiple content blocks.
/// </summary>
[McpServerToolType]
public static class StructuredOutputTool
{
    [McpServerTool(Name = "analyze_text", ReadOnly = true, Idempotent = true, UseStructuredContent = true)]
    [Description("Analyzes input text and returns structured statistics (character count, word count, line count).")]
    public static CallToolResult AnalyzeText(
        [Description("The text to analyze")] string text)
    {
        var charCount = text.Length;
        var wordCount = text.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries).Length;
        var lineCount = text.Split('\n').Length;

        return new CallToolResult
        {
            Content =
            [
                new TextContentBlock
                {
                    Text = $"Characters: {charCount}\nWords: {wordCount}\nLines: {lineCount}"
                },
                new TextContentBlock
                {
                    Text = $"The text contains {wordCount} word(s) across {lineCount} line(s)."
                }
            ]
        };
    }
}
