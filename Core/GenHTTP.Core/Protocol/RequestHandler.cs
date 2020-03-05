using System;
using System.IO;

using GenHTTP.Api.Infrastructure;
using GenHTTP.Api.Modules;
using GenHTTP.Api.Protocol;
using GenHTTP.Api.Routing;

using GenHTTP.Core.Routing;
using GenHTTP.Modules.Core.General;
using GenHTTP.Modules.Core;

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
            IRoutingContext? routing;
            IResponse? response;

            Exception? cause;

            if (TryRoute(request, out routing, out cause))
            {
                response = TryProvideContent(request, routing, out cause);
            }
            else
            {
                // with no routing context, we can't provide a templated error page
                // provide a default error page in this case, if possible
                response = CoreError(request, cause);
            }

            if (response == null)
            {
                // the content provider threw an exception
                // send a templated error message page, if possible
                response = ServerError(request, routing, cause);
            }

            if (!TryIntercept(request, response, out var extensionError))
            {
                // an extension threw an exception
                // send a templated error message
                if (extensionError != null)
                {
                    cause = extensionError;
                }

                response = ServerError(request, routing, cause);
            }

            error = cause;
            return response;
        }

        private bool TryRoute(IRequest request, out IRoutingContext? routingContext, out Exception? error)
        {
            try
            {
                var routing = new RoutingContext(Server.Router, request);
                request.Routing = routing;

                Server.Router.HandleContext(routing);

                foreach (var extension in Server.Extensions)
                {
                    var content = extension.Intercept(request);

                    if (content != null)
                    {
                        routing.RegisterContent(content);
                        break;
                    }
                }

                routingContext = routing;
                error = null;

                return true;
            }
            catch (Exception e)
            {
                routingContext = null;
                error = e;

                return false;
            }
        }

        private IResponse? TryProvideContent(IRequest request, IRoutingContext? routing, out Exception? error)
        {
            if (routing == null)
            {
                error = null;
                return null;
            }

            try
            {
                error = null;

                if (routing.ContentProvider != null)
                {
                    return routing.ContentProvider.Handle(request).Build();
                }
                else
                {
                    return routing.Router.GetErrorHandler(request, ResponseStatus.NotFound, null)
                                         .Handle(request)
                                         .Status(ResponseStatus.NotFound)
                                         .Build();
                }
            }
            catch (Exception e)
            {
                error = e;
                return null;
            }
        }

        private bool TryIntercept(IRequest request, IResponse response, out Exception? error)
        {
            foreach (var extension in Server.Extensions)
            {
                try
                {
                    if (request.Content != null)
                    {
                        request.Content.Seek(0, SeekOrigin.Begin);
                    }

                    extension.Intercept(request, response);
                }
                catch (Exception e)
                {
                    Server.Companion?.OnServerError(ServerErrorScope.Extension, e);

                    error = e;
                    return false;
                }
            }

            error = null;
            return true;
        }

        private IResponse ServerError(IRequest request, IRoutingContext? routing, Exception? cause)
        {
            if (routing != null)
            {
                try
                {
                    return routing.Router.GetErrorHandler(request, DeriveStatus(cause), cause)
                                         .Handle(request)
                                         .Status(DeriveStatus(cause))
                                         .Build();
                }
                catch (Exception e)
                {
                    Server.Companion?.OnServerError(ServerErrorScope.PageGeneration, e);
                }
            }

            return CoreError(request, cause);
        }

        private IResponse CoreError(IRequest request, Exception? cause)
        {
            var coreRouter = Server.Router as CoreRouter;

            if (coreRouter != null)
            {
                try
                {
                    return coreRouter.GetErrorHandler(request, DeriveStatus(cause), cause)
                                     .Handle(request)
                                     .Status(DeriveStatus(cause))
                                     .Build();
                }
                catch (Exception e)
                {
                    Server.Companion?.OnServerError(ServerErrorScope.PageGeneration, e);
                }
            }

            return GenericError(request, cause);
        }

        private IResponse GenericError(IRequest request, Exception? cause)
        {
            return request.Respond()
                          .Status(DeriveStatus(cause))
                          .Content(new StringContent("Internal Server Error"))
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
