using GenHTTP.Modules.ApiBrowsing.Common;
using GenHTTP.Modules.Layouting.Provider;

namespace GenHTTP.Modules.ApiBrowsing;

public static class ApiBrowser
{

    #region Factories

    public static BrowserHandlerBuilder SwaggerUI() => new("Swagger");

    public static BrowserHandlerBuilder Redoc() => new("Redoc");

    #endregion

    #region Layout extensions

    public static LayoutBuilder AddSwaggerUI(this LayoutBuilder layout, string segment = "swagger", string? url = null)
        => AddBrowser(layout, SwaggerUI(), segment, url);

    public static LayoutBuilder AddRedoc(this LayoutBuilder layout, string segment = "redoc", string? url = null)
        => AddBrowser(layout, Redoc(), segment, url);

    private static LayoutBuilder AddBrowser(this LayoutBuilder layout, BrowserHandlerBuilder builder, string segment, string? url)
    {
        if (url != null)
        {
            builder.Url(url);
        }

        return layout.Add(segment, builder);
    }

    #endregion

}
