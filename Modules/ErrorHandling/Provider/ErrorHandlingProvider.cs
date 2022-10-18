using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using GenHTTP.Api.Content;
using GenHTTP.Api.Content.Templating;
using GenHTTP.Api.Protocol;

using GenHTTP.Modules.Pages;

namespace GenHTTP.Modules.ErrorHandling.Provider
{

    public sealed class ErrorHandlingProvider<T> : IConcern where T : Exception
    {

        #region Get-/Setters

        public IHandler Content { get; }

        public IHandler Parent { get; }

        private IErrorHandler<T> ErrorHandler { get; }

        #endregion

        #region Initialization

        public ErrorHandlingProvider(IHandler parent, Func<IHandler, IHandler> contentFactory, IErrorHandler<T> errorHandler)
        {
            Parent = parent;
            Content = contentFactory(this);

            ErrorHandler = errorHandler;
        }

        #endregion

        #region Functionality

        public ValueTask PrepareAsync() => Content.PrepareAsync();

        public IEnumerable<ContentElement> GetContent(IRequest request) => Content.GetContent(request);

        public async ValueTask<IResponse?> HandleAsync(IRequest request)
        {
            try
            {
                var response = await Content.HandleAsync(request)
                                            .ConfigureAwait(false);

                if (response is null)
                {
                    return Content.GetNotFound(request).Build();
                }

                return response;
            }
            catch (ProviderException e)
            {
                var model = new ErrorModel(request, Content, e.Status, e.Message, e);

                var details = ContentInfo.Create()
                                         .Title(e.Status.ToString());

                return this.GetError(model, details.Build()).Build();
            }
            catch (Exception e)
            {
                var model = new ErrorModel(request, Content, ResponseStatus.InternalServerError, "The server failed to handle this request.", e);

                var details = ContentInfo.Create()
                                         .Title("Internal Server Error");

                return this.GetError(model, details.Build()).Build();
            }
        }

        #endregion

    }

}
