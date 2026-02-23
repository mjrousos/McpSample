using System.ComponentModel;
using ModelContextProtocol;
using ModelContextProtocol.Protocol;
using ModelContextProtocol.Server;

namespace DotNetMcpSample.Tools;

/// <summary>
/// Demonstrates the two MCP error-handling patterns:
/// <list type="bullet">
///   <item>
///     <term><see cref="McpException"/></term>
///     <description>
///       Thrown for hard errors where the tool cannot produce a meaningful result —
///       for example, invalid or missing required arguments. The SDK catches the exception
///       and returns an MCP error response to the client, signaling a protocol-level failure.
///     </description>
///   </item>
///   <item>
///     <term><see cref="CallToolResult"/> with <see cref="CallToolResult.IsError"/> = <see langword="true"/></term>
///     <description>
///       Returned for soft, recoverable errors where the tool ran to completion but the
///       operation itself failed — for example, a value that couldn't be parsed. The tool
///       call is considered successful at the protocol level; the AI model receives the error
///       message in the content and can decide how to proceed (e.g., retry with corrected input).
///     </description>
///   </item>
/// </list>
/// </summary>
[McpServerToolType]
public static class ErrorHandlingTool
{
    /// <summary>
    /// Divides two numbers. Throws <see cref="McpException"/> if the divisor is zero,
    /// demonstrating how to signal a hard, unrecoverable error to MCP clients.
    /// </summary>
    [McpServerTool(Name = "divide", ReadOnly = true, Idempotent = true)]
    [Description(
        "Divides dividend by divisor and returns the result. " +
        "Throws an McpException (hard error) if the divisor is zero.")]
    public static string Divide(
        [Description("The dividend")] double dividend,
        [Description("The divisor — must not be zero")] double divisor)
    {
        if (divisor == 0)
        {
            throw new McpException("Division by zero is not allowed. Provide a non-zero divisor.");
        }

        return $"Result: {dividend / divisor}";
    }

    /// <summary>
    /// Parses an integer from a string. Returns a <see cref="CallToolResult"/> with
    /// <see cref="CallToolResult.IsError"/> set to <see langword="true"/> when the
    /// input cannot be parsed, demonstrating how to signal a soft, recoverable error.
    /// </summary>
    [McpServerTool(Name = "safe_parse_int", ReadOnly = true, Idempotent = true)]
    [Description(
        "Parses an integer from a string. " +
        "Returns a CallToolResult with IsError = true (soft error) when the input is not a valid integer, " +
        "allowing the model to retry with corrected input.")]
    public static CallToolResult SafeParseInt(
        [Description("The string to parse as an integer")] string input)
    {
        if (int.TryParse(input, out int value))
        {
            return new CallToolResult
            {
                Content = [new TextContentBlock { Text = $"Parsed value: {value}" }]
            };
        }

        return new CallToolResult
        {
            IsError = true,
            Content = [new TextContentBlock { Text = $"'{input}' is not a valid integer. Please provide a whole number." }]
        };
    }
}
