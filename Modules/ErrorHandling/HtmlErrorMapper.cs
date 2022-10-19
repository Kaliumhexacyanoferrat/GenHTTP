using System;
using System.Threading.Tasks;

using GenHTTP.Api.Content;
using GenHTTP.Api.Content.Templating;
using GenHTTP.Api.Protocol;

using GenHTTP.Modules.Pages;

namespace GenHTTP.Modules.ErrorHandling
{

    /// <summary>
    /// An error mapper that renders exceptions into the currently
    /// installed template, using the next IErrorRenderer found
    /// in the routing chain.
    /// </summary>
    public class HtmlErrorMapper : IErrorMapper<Exception>
    {

        public ValueTask<IResponse?> Map(IRequest request, IHandler handler, Exception error)
        {
            if (error is ProviderException e)
            {
                var model = new ErrorModel(request, handler, e.Status, e.Message, e);

                var details = ContentInfo.Create()
                                         .Title(e.Status.ToString());

                return new(handler.GetError(model, details.Build()).Build());
            }
            else
            {
                var model = new ErrorModel(request, handler, ResponseStatus.InternalServerError, "The server failed to handle this request.", error);

                var details = ContentInfo.Create()
                                         .Title("Internal Server Error");

                return new(handler.GetError(model, details.Build()).Build());
            }
        }

        public ValueTask<IResponse?> GetNotFound(IRequest request, IHandler handler)
        {
            return new(handler.GetNotFound(request).Build());
        }

    }

}
