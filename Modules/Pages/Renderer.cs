using GenHTTP.Api.Content.IO;

using GenHTTP.Modules.IO;
using GenHTTP.Modules.Pages.Rendering;

namespace GenHTTP.Modules.Pages;

/// <summary>
/// Thin wrappers to render HTML pages using the Cottle framework. 
/// </summary>
/// <remarks>
/// To serve a page, you might want to use <see cref="Extensions.GetPage(Api.Protocol.IRequest, string)" />.
/// </remarks>
public static class Renderer
{
    private static readonly ServerRenderer _ServerRenderer = new();

    /// <summary>
    /// A renderer which can be used to generate a HTML page in the
    /// server theme passing just title and content.
    /// </summary>
    public static ServerRenderer Server { get => _ServerRenderer; }

    /// <summary>
    /// Creates a new renderer for the given template file.
    /// </summary>
    /// <param name="template">The template to be used for rendering</param>
    /// <returns>The newly created renderer</returns>
    public static TemplateRenderer From(IResource template) => new(template.Track());

}
