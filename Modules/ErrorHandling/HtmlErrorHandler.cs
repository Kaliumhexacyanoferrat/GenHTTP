using GenHTTP.Api.Content;
using GenHTTP.Api.Content.Templating;
using GenHTTP.Api.Protocol;
using GenHTTP.Modules.Pages;
using System;
using System.Threading.Tasks;

namespace GenHTTP.Modules.ErrorHandling
{

    public class HtmlErrorHandler : IErrorHandler<Exception>
    {

        public ValueTask<IResponse?> Map(IRequest request, IHandler handler, Exception error)
        {
            if (error is ProviderException e)
            {
                var model = new ErrorModel(request, handler, e.Status, e.Message, e);

                var details = ContentInfo.Create()
                                         .Title(e.Status.ToString());

                return this.GetError(model, details.Build()).Build();
            }

            var model = new ErrorModel(request, handler, ResponseStatus.InternalServerError, "The server failed to handle this request.", e);

            var details = ContentInfo.Create()
                                     .Title("Internal Server Error");

            return this.GetError(model, details.Build()).Build();
        }

        public IResponse? GetNotFound(IRequest request, IHandler handler)
        {
            return handler.GetNotFound(request).Build();
        }

    }

}
