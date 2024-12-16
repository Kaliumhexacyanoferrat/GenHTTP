using GenHTTP.Modules.ApiBrowsing.Common;
using GenHTTP.Modules.Layouting.Provider;

namespace GenHTTP.Modules.ApiBrowsing;

public static class Extensions
{

    /// <summary>
    /// Creates a Swagger UI application and registers it at the layout.
    /// </summary>
    /// <param name="layout">The layout to add the application to</param>
    /// <param name="segment">The path to make the application available from (defaults to "/swagger/")</param>
    /// <param name="url">The URL of the Open API definition to be rendered (defaults to "../openapi.json")</param>
    /// <param name="title">The title of the rendered application</param>
    /// <returns>The layout once again (builder pattern)</returns>
    /// <remarks>
    /// There is no auto-detection of Open API definitions provided by the server
    /// so the URL provided needs to point to the correct definition to be consumed.
    /// Use relative paths to avoid issues with CORS, proxies etc.
    /// </remarks>
    public static LayoutBuilder AddSwaggerUI(this LayoutBuilder layout, string segment = "swagger", string? url = null, string? title = null)
        => AddBrowser(layout, ApiBrowser.SwaggerUI(), segment, url, title);

    /// <summary>
    /// Creates a Redoc application and registers it at the layout.
    /// </summary>
    /// <param name="layout">The layout to add the application to</param>
    /// <param name="segment">The path to make the application available from (defaults to "/redoc/")</param>
    /// <param name="url">The URL of the Open API definition to be rendered (defaults to "../openapi.json")</param>
    /// <param name="title">The title of the rendered application</param>
    /// <returns>The layout once again (builder pattern)</returns>
    /// <remarks>
    /// There is no auto-detection of Open API definitions provided by the server
    /// so the URL provided needs to point to the correct definition to be consumed.
    /// Use relative paths to avoid issues with CORS, proxies etc.
    /// </remarks>
    public static LayoutBuilder AddRedoc(this LayoutBuilder layout, string segment = "redoc", string? url = null, string? title = null)
        => AddBrowser(layout, ApiBrowser.Redoc(), segment, url, title);

    /// <summary>
    /// Creates a Scalar application and registers it at the layout.
    /// </summary>
    /// <param name="layout">The layout to add the application to</param>
    /// <param name="segment">The path to make the application available from (defaults to "/scalar/")</param>
    /// <param name="url">The URL of the Open API definition to be rendered (defaults to "../openapi.json")</param>
    /// <param name="title">The title of the rendered application</param>
    /// <returns>The layout once again (builder pattern)</returns>
    /// <remarks>
    /// There is no auto-detection of Open API definitions provided by the server
    /// so the URL provided needs to point to the correct definition to be consumed.
    /// Use relative paths to avoid issues with CORS, proxies etc.
    /// </remarks>
    public static LayoutBuilder AddScalar(this LayoutBuilder layout, string segment = "scalar", string? url = null, string? title = null)
        => AddBrowser(layout, ApiBrowser.Scalar(), segment, url, title);

    private static LayoutBuilder AddBrowser(this LayoutBuilder layout, BrowserHandlerBuilder builder, string segment, string? url, string? title)
    {
        if (url != null)
        {
            builder.Url(url);
        }

        if (title != null)
        {
            builder.Title(title);
        }

        return layout.Add(segment, builder);
    }

}
