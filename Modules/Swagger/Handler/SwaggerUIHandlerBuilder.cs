using GenHTTP.Api.Content;

namespace GenHTTP.Modules.Swagger.Handler;

public sealed class SwaggerUIHandlerBuilder : IHandlerBuilder<SwaggerUIHandlerBuilder>
{
    private readonly List<IConcernBuilder> _Concerns = [];

    private string? _Url;

    public SwaggerUIHandlerBuilder Url(string url)
    {
        _Url = url;
        return this;
    }

    public SwaggerUIHandlerBuilder Add(IConcernBuilder concern)
    {
        _Concerns.Add(concern);
        return this;
    }

    public IHandler Build()
    {
        return Concerns.Chain(_Concerns, new SwaggerUIHandler(_Url));
    }

}
