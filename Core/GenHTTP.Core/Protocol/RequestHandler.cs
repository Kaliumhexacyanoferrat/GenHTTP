using GenHTTP.Api.Content;
using GenHTTP.Api.Infrastructure;
using GenHTTP.Api.Protocol;
using GenHTTP.Modules.Core;
using GenHTTP.Modules.Core.General;
using System;

namespace GenHTTP.Core.Protocol
{

    internal class RequestHandler
    {

        #region Get-/Setters

        private IServer Server { get; }

        #endregion

        #region Initialization

        internal RequestHandler(IServer server)
        {
            Server = server;
        }

        #endregion

        #region Functionality

        internal IResponse Handle(IRequest request, out Exception? error)
        {
            try
            {
                error = null;
                return Server.Handler.Handle(request) ?? NotFound(request);
            }
            catch (Exception e)
            {
                error = e;
                return GenericError(request, error);
            }
        }

        private IResponse NotFound(IRequest request) => Respond(request, ResponseStatus.NotFound);

        private IResponse GenericError(IRequest request, Exception? cause) => Respond(request, DeriveStatus(cause));

        private IResponse Respond(IRequest request, ResponseStatus status)
        {
            return request.Respond()
                          .Status(ResponseStatus.NotFound)
                          .Content(new StringContent(status.ToString()))
                          .Type(ContentType.TextPlain)
                          .Build();
        }

        private ResponseStatus DeriveStatus(Exception? exception)
        {
            if (exception is ProviderException pe)
            {
                return pe.Status;
            }

            return ResponseStatus.InternalServerError;
        }

        #endregion

    }

}
