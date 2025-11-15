using GenHTTP.Modules.ApiBrowsing.Common;

namespace GenHTTP.Modules.ApiBrowsing;

/// <summary>
/// Provides graphical, JavaScript based web applications that render an Open API
/// definition so that the API can be explored by users.
/// </summary>
public static class ApiBrowser
{

    /// <summary>
    /// Creates a handler that will provide a Swagger UI app.
    /// </summary>
    /// <returns>The newly created handler</returns>
    public static BrowserHandlerBuilder SwaggerUi() => new("Swagger", "Swagger UI");

    /// <summary>
    /// Creates a handler that will provide a Redoc app.
    /// </summary>
    /// <returns>The newly created handler</returns>
    public static BrowserHandlerBuilder Redoc() => new("Redoc", "Redoc");

    /// <summary>
    /// Creates a handler that will provide a Scalar app.
    /// </summary>
    /// <returns>The newly created handler</returns>
    public static BrowserHandlerBuilder Scalar() => new("Scalar", "Scalar");

}
