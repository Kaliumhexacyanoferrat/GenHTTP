using GenHTTP.Modules.ApiBrowsing.Common;
using GenHTTP.Modules.Layouting.Provider;

namespace GenHTTP.Modules.ApiBrowsing;

public static class ApiBrowser
{

    #region Factories

    public static BrowserHandlerBuilder SwaggerUI() => new("Swagger", "Swagger UI");

    public static BrowserHandlerBuilder Redoc() => new("Redoc", "Redoc");

    #endregion

    #region Layout extensions

    public static LayoutBuilder AddSwaggerUI(this LayoutBuilder layout, string segment = "swagger", string? url = null, string? title = null)
        => AddBrowser(layout, SwaggerUI(), segment, url, title);

    public static LayoutBuilder AddRedoc(this LayoutBuilder layout, string segment = "redoc", string? url = null, string? title = null)
        => AddBrowser(layout, Redoc(), segment, url, title);

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

    #endregion

}
