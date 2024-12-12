using GenHTTP.Modules.Layouting.Provider;
using GenHTTP.Modules.Swagger.Handler;

namespace GenHTTP.Modules.Swagger;

public static class SwaggerUI
{

    public static SwaggerUIHandlerBuilder Create() => new();

    public static LayoutBuilder AddSwaggerUI(this LayoutBuilder layout, string segment = "swagger", string? url = null)
        => layout.Add(segment, (url != null) ? Create().Url(url) : Create());

}
