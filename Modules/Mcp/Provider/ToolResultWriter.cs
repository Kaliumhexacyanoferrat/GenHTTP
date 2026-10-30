using System.Text.Json;

namespace GenHTTP.Modules.Mcp.Provider;

/// <summary>
/// Writes the outcome of a tool call as the body of a call result.
/// </summary>
/// <remarks>
/// JSON data is sent as structured content and, for clients that only read the
/// content blocks, also as text. Legacy protocol versions only accept objects
/// as structured content, scalar values are only sent as text.
/// </remarks>
internal static class ToolResultWriter
{

    internal static void Write(Utf8JsonWriter writer, in ToolOutcome outcome, bool modern)
    {
        var content = outcome.Result.Content;
        var structured = outcome.Structured;

        writer.WriteStartArray("content"u8);

        if (content.Count == 0 && structured != null)
        {
            writer.WriteStartObject();
            writer.WriteString("type"u8, "text"u8);
            writer.WriteString("text"u8, structured.WrittenSpan);
            writer.WriteEndObject();
        }

        foreach (var block in content)
        {
            block.Write(writer);
        }

        writer.WriteEndArray();

        if (structured != null)
        {
            var json = structured.WrittenSpan;

            if (json.Length > 0 && (json[0] == (byte)'{' || (modern && json[0] == (byte)'[')))
            {
                writer.WritePropertyName("structuredContent"u8);
                writer.WriteRawValue(json, skipInputValidation: true);
            }
        }

        writer.WriteBoolean("isError"u8, outcome.Result.IsError);
    }

}
