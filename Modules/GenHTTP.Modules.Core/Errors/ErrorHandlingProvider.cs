using System;
using System.Collections.Generic;

using GenHTTP.Api.Content;
using GenHTTP.Api.Content.Templating;
using GenHTTP.Api.Protocol;
using GenHTTP.Api.Routing;

namespace GenHTTP.Modules.Core.Errors
{

    public class ErrorHandlingProvider : IConcern
    {

        #region Get-/Setters

        public IHandler Content { get; }

        public IHandler Parent { get; }

        #endregion

        #region Initialization

        public ErrorHandlingProvider(IHandler parent, Func<IHandler, IHandler> contentFactory)
        {
            Parent = parent;
            Content = contentFactory(this);
        }

        #endregion

        #region Functionality

        public IEnumerable<ContentElement> GetContent(IRequest request) => Content.GetContent(request);

        public IResponse? Handle(IRequest request)
        {
            try
            {
                var response = Content.Handle(request);

                if (response == null)
                {
                    return this.NotFound(request)
                               .Build();
                }

                return response;
            }
            catch (ProviderException e)
            {
                var model = new ErrorModel(request, this, e.Status, e.Status.ToString(), e.Message, e);

                return this.Error(model)
                           .Build();
            }
            catch (Exception e)
            {
                var model = new ErrorModel(request, this, ResponseStatus.InternalServerError,
                                           "Internal Server Error", "The server failed to handle this request.", e);

                return this.Error(model)
                           .Build();
            }
        }

        #endregion

    }

}
