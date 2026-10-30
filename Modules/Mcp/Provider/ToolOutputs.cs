using System.Text.Json;
using System.Text.Json.Serialization.Metadata;

using GenHTTP.Api.Content;

using GenHTTP.Modules.Mcp.Protocol;

namespace GenHTTP.Modules.Mcp.Provider;

/// <summary>
/// Converts whatever a tool returned or threw into an outcome.
/// </summary>
internal static class ToolOutputs
{
    private static readonly ToolResult NoContent = new([]);

    internal static ToolOutcome Capture(object? output, JsonTypeInfo<object?> typeInfo)
    {
        if (output is string text)
        {
            return new(ToolResult.Text(text));
        }

        if (output is ToolResult result)
        {
            return result.Structured != null ? Serialize(result, result.Structured, typeInfo) : new(result);
        }

        return Serialize(NoContent, output, typeInfo);
    }

    internal static ToolOutcome Empty() => new(NoContent);

    internal static ToolOutcome Fail(Exception exception) => exception is ProviderException
        ? ToolOutcome.Failed(exception.Message)
        : ToolOutcome.Crashed(exception);

    private static ToolOutcome Serialize(ToolResult result, object? data, JsonTypeInfo<object?> typeInfo)
    {
        var buffer = JsonBuffer.Rent();

        try
        {
            JsonSerializer.Serialize(buffer.Writer, data, typeInfo);
            buffer.Flush();
        }
        catch (Exception e)
        {
            buffer.Dispose();
            return ToolOutcome.Crashed(e);
        }

        return new(result, buffer);
    }

}
