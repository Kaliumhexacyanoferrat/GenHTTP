using System.Text.Json;

namespace GenHTTP.Modules.Mcp;

/// <summary>
/// The result of a tool call, allowing full control over the content
/// sent to the client.
/// </summary>
/// <remarks>
/// Tools can also return a <see cref="string" /> (sent as text) or any other
/// object (sent as JSON, both as text and as structured content).
/// </remarks>
public sealed class ToolResult
{
    private static readonly IReadOnlyList<ToolContent> NoContent = [];

    #region Get-/Setters

    /// <summary>
    /// The content blocks to be presented to the model.
    /// </summary>
    public IReadOnlyList<ToolContent> Content { get; }

    /// <summary>
    /// Data to be sent as structured content, serialized to JSON.
    /// </summary>
    /// <remarks>
    /// If no content blocks are given, the serialized data will also be
    /// sent as a text block, as recommended by the specification.
    /// </remarks>
    public object? Structured { get; }

    /// <summary>
    /// Whether the call failed.
    /// </summary>
    public bool IsError { get; }

    #endregion

    #region Initialization

    /// <summary>
    /// Creates a new result.
    /// </summary>
    /// <param name="content">The content blocks to be presented to the model</param>
    /// <param name="structured">Data to be sent as structured content</param>
    /// <param name="isError">Whether the call failed</param>
    public ToolResult(IReadOnlyList<ToolContent>? content, object? structured = null, bool isError = false)
    {
        Content = content ?? NoContent;
        Structured = structured;
        IsError = isError;
    }

    /// <summary>
    /// A result consisting of a single text block, such as Markdown.
    /// </summary>
    /// <param name="text">The text to be read by the model</param>
    public static ToolResult Text(string text) => new([new TextContent(text)]);

    /// <summary>
    /// A result consisting of the given content blocks.
    /// </summary>
    /// <param name="content">The content blocks to be presented to the model</param>
    public static ToolResult From(params ToolContent[] content) => new(content);

    /// <summary>
    /// A result carrying the given data as JSON.
    /// </summary>
    /// <param name="data">The data to be serialized</param>
    public static ToolResult Json(object data) => new(null, data);

    /// <summary>
    /// A failed call, with an explanation for the model.
    /// </summary>
    /// <param name="message">The reason the call failed</param>
    public static ToolResult Error(string message) => new([new TextContent(message)], isError: true);

    #endregion

}

/// <summary>
/// A block of content returned by a tool.
/// </summary>
public abstract record ToolContent
{

    private protected ToolContent() { }

    internal abstract void Write(Utf8JsonWriter writer);

}

/// <summary>
/// Text, such as plain text or Markdown.
/// </summary>
/// <param name="Text">The text to be read by the model</param>
public sealed record TextContent(string Text) : ToolContent
{

    internal override void Write(Utf8JsonWriter writer)
    {
        writer.WriteStartObject();
        writer.WriteString("type"u8, "text"u8);
        writer.WriteString("text"u8, Text);
        writer.WriteEndObject();
    }

}

/// <summary>
/// An image.
/// </summary>
/// <param name="Data">The binary data of the image</param>
/// <param name="MimeType">The type of the image, such as "image/png"</param>
public sealed record ImageContent(ReadOnlyMemory<byte> Data, string MimeType) : ToolContent
{

    internal override void Write(Utf8JsonWriter writer) => BinaryContent.Write(writer, "image"u8, Data, MimeType);

}

/// <summary>
/// An audio clip.
/// </summary>
/// <param name="Data">The binary data of the clip</param>
/// <param name="MimeType">The type of the clip, such as "audio/wav"</param>
public sealed record AudioContent(ReadOnlyMemory<byte> Data, string MimeType) : ToolContent
{

    internal override void Write(Utf8JsonWriter writer) => BinaryContent.Write(writer, "audio"u8, Data, MimeType);

}

/// <summary>
/// A link to a resource the client can fetch.
/// </summary>
/// <param name="Uri">The URI of the resource</param>
/// <param name="Name">The name of the resource</param>
public sealed record ResourceLinkContent(string Uri, string Name) : ToolContent
{

    /// <summary>
    /// A human-readable name of the resource.
    /// </summary>
    public string? Title { get; init; }

    /// <summary>
    /// Describes what the resource contains.
    /// </summary>
    public string? Description { get; init; }

    /// <summary>
    /// The type of the resource, if known.
    /// </summary>
    public string? MimeType { get; init; }

    internal override void Write(Utf8JsonWriter writer)
    {
        writer.WriteStartObject();
        writer.WriteString("type"u8, "resource_link"u8);
        writer.WriteString("uri"u8, Uri);
        writer.WriteString("name"u8, Name);

        if (Title != null)
        {
            writer.WriteString("title"u8, Title);
        }

        if (Description != null)
        {
            writer.WriteString("description"u8, Description);
        }

        if (MimeType != null)
        {
            writer.WriteString("mimeType"u8, MimeType);
        }

        writer.WriteEndObject();
    }

}

internal static class BinaryContent
{

    internal static void Write(Utf8JsonWriter writer, ReadOnlySpan<byte> type, ReadOnlyMemory<byte> data, string mimeType)
    {
        writer.WriteStartObject();
        writer.WriteString("type"u8, type);
        writer.WriteBase64String("data"u8, data.Span);
        writer.WriteString("mimeType"u8, mimeType);
        writer.WriteEndObject();
    }

}
