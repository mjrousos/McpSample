using System.ComponentModel;
using ModelContextProtocol.Server;

namespace DotNetMcpSample.Tools;

/// <summary>
/// Demonstrates the MCP structured output pattern using <c>UseStructuredContent = true</c>.
/// </summary>
/// <remarks>
/// <para>
/// <b>How structured output works in the C# MCP SDK:</b>
/// </para>
/// <para>
/// When <see cref="McpServerToolAttribute.UseStructuredContent"/> is set to <c>true</c>, the SDK:
/// <list type="number">
///   <item>Generates an <c>outputSchema</c> from the tool method's return type and advertises it
///         to clients during tool discovery (<c>tools/list</c>).</item>
///   <item>After the tool executes, serializes the return value as JSON text into a
///         <c>TextContentBlock</c> in the response's <c>Content</c> array (for backward compatibility
///         with clients that don't understand structured content).</item>
///   <item>Also populates the <c>StructuredContent</c> field with the typed JSON object, which
///         clients that support <c>outputSchema</c> can use for reliable, schema-validated parsing.</item>
/// </list>
/// </para>
/// <para>
/// <b>Important:</b> The tool method must return a <b>POCO (plain C# object)</b> — not
/// <see cref="ModelContextProtocol.Protocol.CallToolResult"/>. If you return <c>CallToolResult</c>
/// directly, the SDK (a) generates the output schema from <c>CallToolResult</c>'s own shape
/// (which includes internal protocol properties like <c>content</c>, <c>structuredContent</c>,
/// <c>isError</c>) producing an invalid schema that clients reject, and (b) returns the
/// <c>CallToolResult</c> as-is without applying any structured content processing.
/// </para>
/// <para>
/// <b>Client behavior:</b> Clients that support <c>outputSchema</c> will receive both the typed
/// <c>structuredContent</c> JSON and the text fallback. Clients without <c>outputSchema</c> support
/// will still receive the serialized JSON as text in the <c>content</c> array, so the tool remains
/// functional across all MCP clients.
/// </para>
/// </remarks>
[McpServerToolType]
public static class StructuredOutputTool
{
    [McpServerTool(Name = "analyze_text", ReadOnly = true, Idempotent = true, UseStructuredContent = true)]
    [Description("Analyzes input text and returns structured statistics (character count, word count, line count).")]
    public static TextAnalysisResult AnalyzeText(
        [Description("The text to analyze")] string text)
    {
        return new TextAnalysisResult
        {
            Characters = text.Length,
            Words = text.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries).Length,
            Lines = text.Split('\n').Length
        };
    }
}

/// <summary>
/// Strongly-typed result for <see cref="StructuredOutputTool.AnalyzeText"/>.
/// The SDK uses this type to generate the <c>outputSchema</c> and to populate
/// <c>StructuredContent</c> on the MCP response.
/// </summary>
public class TextAnalysisResult
{
    [Description("Total number of characters in the input text")]
    public int Characters { get; set; }

    [Description("Number of whitespace-delimited words")]
    public int Words { get; set; }

    [Description("Number of lines (newline-delimited)")]
    public int Lines { get; set; }
}
