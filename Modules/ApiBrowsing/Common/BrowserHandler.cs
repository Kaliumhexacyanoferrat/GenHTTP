using Cottle;
using GenHTTP.Api.Content;
using GenHTTP.Api.Protocol;
using GenHTTP.Modules.Basics;
using GenHTTP.Modules.IO;
using GenHTTP.Modules.Pages;
using GenHTTP.Modules.Pages.Rendering;

namespace GenHTTP.Modules.ApiBrowsing.Common;

public sealed class BrowserHandler: IHandler
{

    #region Get-/Setters

    public IHandler StaticResources { get; }

    public TemplateRenderer Template { get; }

    public BrowserMetaData MetaData { get; }

    #endregion

    #region Initialization

    public BrowserHandler(string resourceRoot, BrowserMetaData metaData)
    {
        StaticResources = Resources.From(ResourceTree.FromAssembly($"{resourceRoot}.Static"))
                                   .Build();

        Template = Renderer.From(Resource.FromAssembly($"{resourceRoot}.Index.html").Build());

        MetaData = metaData;
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
                ["title"] = MetaData.Title,
                ["url"] = (MetaData.Url ?? "../openapi.json")
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
