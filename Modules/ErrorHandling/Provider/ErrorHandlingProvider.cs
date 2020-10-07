using System;
using System.Collections.Generic;

using GenHTTP.Api.Content;
using GenHTTP.Api.Content.Templating;
using GenHTTP.Api.Protocol;

using GenHTTP.Modules.Basics;

namespace GenHTTP.Modules.ErrorHandling.Provider
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
                    return Content.NotFound(request)
                                  .Build();
                }

                return response;
            }
            catch (ProviderException e)
            {
                var model = new ErrorModel(request, Content, e.Status, e.Message, e);

                var details = ContentInfo.Create()
                                         .Title(e.Status.ToString());

                return this.Error(model, details.Build())
                           .Build();
            }
            catch (Exception e)
            {
                var model = new ErrorModel(request, Content, ResponseStatus.InternalServerError, "The server failed to handle this request.", e);

                var details = ContentInfo.Create()
                                         .Title("Internal Server Error");

                return this.Error(model, details.Build())
                           .Build();
            }
        }

        #endregion

    }

}
