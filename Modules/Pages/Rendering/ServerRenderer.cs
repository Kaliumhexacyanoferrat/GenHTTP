using Cottle;
using GenHTTP.Api.Content.IO;
using GenHTTP.Modules.IO;

namespace GenHTTP.Modules.Pages.Rendering;

public sealed class ServerRenderer
{
    private static readonly IResource _ServerTemplate = Resource.FromAssembly("ServerPage.html").Build();

    private readonly TemplateRenderer _TemplateRender;

    public ServerRenderer()
    {
        _TemplateRender = Renderer.From(_ServerTemplate);
    }

    /// <summary>
    ///     Renders a server-styled HTML page for the given title and content.
    /// </summary>
    /// <param name="title">The title of the page to be rendered</param>
    /// <param name="content">The HTML content of the page</param>
    /// <returns>The generated HTML page</returns>
    /// <remarks>
    ///     This method will not escape the given title or content.
    /// </remarks>
    public async ValueTask<string> RenderAsync(string title, string content) => await _TemplateRender.RenderAsync(new Dictionary<Value, Value>
    {
        ["title"] = title,
        ["content"] = content
    });

    /// <summary>
    ///     Renders a server-styled HTML error page for the given exception.
    /// </summary>
    /// <param name="title">The title of the page to be rendered</param>
    /// <param name="error">The error which has ocurred</param>
    /// <param name="developmentMode">Whether additional error information should be printed</param>
    /// <returns>The generated HTML error page</returns>
    public ValueTask<string> RenderAsync(string title, Exception error, bool developmentMode)
        => RenderAsync(title, developmentMode ? error.ToString().Escaped() : error.Message.Escaped());
}
