using System;
using System.Collections.Generic;
using System.Text;

using GenHTTP.Api.Content;
using GenHTTP.Api.Infrastructure;
using GenHTTP.Api.Protocol;
using GenHTTP.Api.Routing;

using GenHTTP.Core.Routing;

namespace GenHTTP.Core.Protocol
{

    internal class RequestHandler
    {

        protected IServer Server { get; }

        protected ClientHandler ClientHandler { get; }

        public RequestHandler(IServer server, ClientHandler clientHandler)
        {
            Server = server;
            ClientHandler = clientHandler;
        }

        internal bool HandleRequest(HttpRequest request, bool keepAlive)
        {
            var response = new HttpResponse(ClientHandler, request.Type == RequestType.HEAD, request.ProtocolType, keepAlive);

            IRoutingContext? routing;
            Exception? error;

            if (TryRoute(request, out routing, out error))
            {
                if (TryProvideContent(request, response, routing, out error))
                {
                    if (!response.Sent)
                    {
                        // the content provider didn't send data, so the
                        // request is still unhandled
                        ServerError(request, response, routing);
                    }
                }
                else
                {
                    // the content provider threw an exception
                    // send a templated error message page, if possible
                    ServerError(request, response, routing);
                }
            }
            else
            {
                // with no routing context, we can't provide a templated error page
                // provide a default error page in this case, if possible
                CoreError(request, response);
            }

            Server.Companion?.OnRequestHandled(request, response, error);

            if (response.Header.CloseConnection)
            {
                return true;
            }

            return !keepAlive;
        }

        protected bool TryRoute(HttpRequest request, out IRoutingContext? routingContext, out Exception? error)
        {
            try
            {
                var routing = new RoutingContext(Server.Router, request);
                request.Routing = routing;

                Server.Router.HandleContext(routing);

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

        protected bool TryProvideContent(HttpRequest request, HttpResponse response, IRoutingContext? routing, out Exception? error)
        {
            if (routing == null)
            {
                error = null;
                return false;
            }

            try
            {
                IContentProvider provider;

                if (routing.ContentProvider != null)
                {
                    provider = routing.ContentProvider;
                }
                else
                {
                    response.Header.Type = ResponseType.NotFound;
                    provider = routing.Router.GetErrorHandler(request, response);
                }

                provider.Handle(request, response);

                error = null;
                return true;
            }
            catch (Exception e)
            {
                error = e;
                return false;
            }
        }

        protected bool ServerError(HttpRequest request, HttpResponse response, IRoutingContext? routing)
        {
            if (response.Sent) return true;

            if (routing != null)
            {
                try
                {
                    response.Header.Type = ResponseType.InternalServerError;

                    routing.Router.GetErrorHandler(request, response)
                                  .Handle(request, response);

                    return true;
                }
                catch (Exception e)
                {
                    Server.Companion?.OnServerError(ServerErrorScope.PageGeneration, e);
                }
            }

            return CoreError(request, response);
        }

        protected bool CoreError(HttpRequest request, HttpResponse response)
        {
            if (response.Sent) return true;

            var coreRouter = Server.Router as CoreRouter;

            if (coreRouter != null)
            {
                try
                {
                    response.Header.Type = ResponseType.InternalServerError;

                    var page = coreRouter.GetPage(request, response);

                    using (var stream = page.GetStream())
                    {
                        response.Send(stream);
                    }

                    return true;
                }
                catch (Exception e)
                {
                    Server.Companion?.OnServerError(ServerErrorScope.PageGeneration, e);
                }
            }

            return GenericError(request, response);
        }

        protected bool GenericError(HttpRequest request, HttpResponse response)
        {
            if (response.Sent) return true;

            response.Header.Type = ResponseType.InternalServerError;
            response.Header.ContentType = ContentType.TextPlain;

            response.Send(Encoding.UTF8.GetBytes("Internal Server Error"));

            return true;
        }

    }

}
