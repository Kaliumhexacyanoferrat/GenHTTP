using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using GenHTTP.Api.Content;
using GenHTTP.Api.Protocol;

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
                    return await ErrorHandler.GetNotFound(request, Content);
                }

                return response;
            }
            catch (T e)
            {
                return await ErrorHandler.Map(request, Content, e);
            }
        }

        #endregion

    }

}
