using Cottle;
using GenHTTP.Api.Content;
using GenHTTP.Api.Protocol;
using GenHTTP.Modules.Basics;
using GenHTTP.Modules.IO;
using GenHTTP.Modules.Pages;
using GenHTTP.Modules.Pages.Rendering;

namespace GenHTTP.Modules.Swagger.Handler;

public sealed class SwaggerUIHandler : IHandler
{

    #region Get-/Setters

    public IHandler StaticResources { get; }

    public TemplateRenderer Template { get; }

    public string Url { get; }

    #endregion

    #region Initialization

    public SwaggerUIHandler(string? url)
    {
        StaticResources = Resources.From(ResourceTree.FromAssembly("Resources.Static"))
                                   .Build();

        Template = Renderer.From(Resource.FromAssembly("Index.html").Build());

        Url = url ?? "../openapi.json";
    }

    #endregion

    #region Functionality

    public ValueTask PrepareAsync() => ValueTask.CompletedTask;

    public async ValueTask<IResponse?> HandleAsync(IRequest request)
    {
        if (!request.HasType(RequestMethod.Get, RequestMethod.Head))
        {
            throw new ProviderException(ResponseStatus.MethodNotAllowed, "Only GET requests are allowed by this handler", (b) => b.Header("Allow", "GET"));
        }

        if (request.Target.Ended)
        {
            var config = new Dictionary<Value, Value>
            {
                ["url"] = Url
            };

            var content = await Template.RenderAsync(config);

            return request.GetPage(content)
                          .Build();
        }

        if (request.Target.Current?.Value == "static")
        {
            request.Target.Advance();
            return await StaticResources.HandleAsync(request);
        }

        return null;
    }

    #endregion

}
