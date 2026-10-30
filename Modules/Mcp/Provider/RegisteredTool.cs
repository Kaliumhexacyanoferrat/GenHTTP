using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;


namespace GenHTTP.Modules.Mcp.Provider;

/// <summary>
/// A tool as offered by the handler, with everything needed to describe and to invoke it.
/// </summary>
internal sealed class RegisteredTool
{

    #region Get-/Setters

    internal string Name { get; }

    internal byte[] EncodedName { get; }

    internal MethodToolInvoker Invoker { get; }

    private string? Title { get; }

    private string Description { get; }

    private ToolHints? Hints { get; }

    private JsonObject InputSchema { get; }

    private JsonObject? OutputSchema { get; }

    #endregion

    #region Initialization

    internal RegisteredTool(string name, string? title, string description, ToolHints? hints, JsonObject inputSchema, JsonObject? outputSchema, MethodToolInvoker invoker)
    {
        ToolNames.Validate(name);

        Name = name;
        EncodedName = Encoding.UTF8.GetBytes(name);

        Title = title;
        Description = description;
        Hints = hints;

        InputSchema = inputSchema;
        OutputSchema = outputSchema;

        Invoker = invoker;
    }

    #endregion

    #region Functionality

    internal void WriteDefinition(Utf8JsonWriter writer)
    {
        writer.WriteStartObject();

        writer.WriteString("name"u8, Name);

        if (Title != null)
        {
            writer.WriteString("title"u8, Title);
        }

        writer.WriteString("description"u8, Description);

        writer.WritePropertyName("inputSchema"u8);
        InputSchema.WriteTo(writer);

        if (OutputSchema != null)
        {
            writer.WritePropertyName("outputSchema"u8);
            OutputSchema.WriteTo(writer);
        }

        if (Hints != null || Title != null)
        {
            writer.WriteStartObject("annotations"u8);

            if (Title != null)
            {
                writer.WriteString("title"u8, Title);
            }

            WriteHint(writer, "readOnlyHint"u8, Hints?.ReadOnly);
            WriteHint(writer, "destructiveHint"u8, Hints?.Destructive);
            WriteHint(writer, "idempotentHint"u8, Hints?.Idempotent);
            WriteHint(writer, "openWorldHint"u8, Hints?.OpenWorld);

            writer.WriteEndObject();
        }

        writer.WriteEndObject();
    }

    private static void WriteHint(Utf8JsonWriter writer, ReadOnlySpan<byte> name, bool? value)
    {
        if (value != null)
        {
            writer.WriteBoolean(name, value.Value);
        }
    }

    #endregion

}

internal static class ToolNames
{

    internal static void Validate(string? name)
    {
        if (string.IsNullOrEmpty(name) || name.Length > 128)
        {
            throw new InvalidOperationException($"The name of a tool needs to be between 1 and 128 characters long, but '{name}' is not.");
        }

        foreach (var c in name)
        {
            if (!char.IsAsciiLetterOrDigit(c) && c != '_' && c != '-' && c != '.')
            {
                throw new InvalidOperationException($"The name of tool '{name}' may only contain ASCII letters, digits, '_', '-' and '.'.");
            }
        }
    }

    /// <summary>
    /// Derives a tool name from a method name, e.g. "create_user" from "CreateUserAsync".
    /// </summary>
    internal static string FromMethod(string method)
    {
        if (method.EndsWith("Async", StringComparison.Ordinal) && method.Length > 5)
        {
            method = method[..^5];
        }

        var name = new StringBuilder(method.Length + 8);

        for (var i = 0; i < method.Length; i++)
        {
            var c = method[i];

            if (char.IsUpper(c) && i > 0)
            {
                var previous = method[i - 1];
                var next = i + 1 < method.Length ? method[i + 1] : '\0';

                if (char.IsLower(previous) || char.IsDigit(previous) || (char.IsUpper(previous) && char.IsLower(next)))
                {
                    name.Append('_');
                }
            }

            name.Append(char.ToLowerInvariant(c));
        }

        return name.ToString();
    }

}
