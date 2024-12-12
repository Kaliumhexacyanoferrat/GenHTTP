using GenHTTP.Api.Content;
using GenHTTP.Api.Protocol;
using GenHTTP.Modules.Basics;
using GenHTTP.Modules.Pages;

namespace GenHTTP.Modules.ErrorHandling;

public class HtmlErrorMapper : IErrorMapper<Exception>
{

    public async ValueTask<IResponse?> Map(IRequest request, IHandler handler, Exception error)
    {
        var developmentMode = request.Server.Development;

        if (error is ProviderException e)
        {
            var title = e.Status.ToString();

            var page = await Renderer.Server.RenderAsync(title, e, developmentMode);

            return request.GetPage(page)
                          .Status(e.Status)
                          .Apply(e.Modifications)
                          .Build();
        }
        else
        {
            var page = await Renderer.Server.RenderAsync("Internal Server Error", error, developmentMode);

            return request.GetPage(page)
                          .Status(ResponseStatus.InternalServerError)
                          .Build();
        }
    }

    public async ValueTask<IResponse?> GetNotFound(IRequest request, IHandler handler)
    {
        var content = await Renderer.Server.RenderAsync("Not Found", "The specified content was not found on this server.");

        return request.GetPage(content)
                      .Status(ResponseStatus.NotFound)
                      .Build();
    }
}
