using GenHTTP.Modules.Mcp.Protocol;

namespace GenHTTP.Modules.Mcp.Provider;

/// <summary>
/// What a tool produced, before it is written as a protocol message.
/// </summary>
/// <param name="Result">The content to be presented to the model</param>
/// <param name="Structured">The result serialized as JSON, if any</param>
/// <param name="Crash">The unexpected exception the call failed with, if any</param>
internal readonly record struct ToolOutcome(ToolResult Result, JsonBuffer? Structured = null, Exception? Crash = null)
{

    internal static ToolOutcome Failed(string message) => new(ToolResult.Error(message));

    internal static ToolOutcome Crashed(Exception exception) => new(ToolResult.Error($"The tool failed unexpectedly: {exception.Message}"), Crash: exception);

}
