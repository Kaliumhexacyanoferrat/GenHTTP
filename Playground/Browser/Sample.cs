using System.Text.Json.Serialization;

using GenHTTP.Api.Content;

namespace GenHTTP.Playground.Browser;

/// <param name="Segment">The path the sample is served at, e.g. "webservices" for http://localhost:8080/webservices/</param>
/// <param name="Source">The file name of the sample within the Samples folder</param>
public sealed record Sample(string Segment, string Category, string Name, string Description, string Documentation, string Source,
                            [property: JsonIgnore] IHandlerBuilder Handler, params Example[] Examples);

/// <summary>
/// A way to try a sample: a request sent from the sample browser, a page
/// opened in the browser or a shell command ("{origin}" is replaced by the
/// URL of the playground).
/// </summary>
public sealed record Example(string Kind, string Title, string? Method = null, string? Path = null, string? Body = null,
                             Dictionary<string, string>? Headers = null, string? Command = null, string? Note = null)
{

    public static Example Request(string title, string method, string path, string? body = null, Dictionary<string, string>? headers = null, string? note = null)
        => new("request", title, method, path, body, headers, Note: note);

    public static Example Page(string title, string path, string? note = null)
        => new("page", title, Path: path, Note: note);

    public static Example Shell(string title, string command, string? note = null)
        => new("shell", title, Command: command, Note: note);

}
