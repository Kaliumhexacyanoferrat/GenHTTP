using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

using GenHTTP.Api.Infrastructure;
using GenHTTP.Api.Modules;
using GenHTTP.Api.Protocol;
using GenHTTP.Api.Routing;

using GenHTTP.Core.Routing;

namespace GenHTTP.Core.Protocol
{

    internal class RequestHandler
    {

        #region Get-/Setters

        protected IServer Server { get; }
        
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

            if (TryRoute(request, out routing, out error))
            {
                response = TryProvideContent(request, routing, out error);                
            }
            else
            {
                // with no routing context, we can't provide a templated error page
                // provide a default error page in this case, if possible
                response = CoreError(request);
            }

            if (response == null)
            {
                // the content provider threw an exception
                // send a templated error message page, if possible
                response = ServerError(request, routing);
            }

            return response;
        }

        protected bool TryRoute(IRequest request, out IRoutingContext? routingContext, out Exception? error)
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

        protected IResponse? TryProvideContent(IRequest request, IRoutingContext? routing, out Exception? error)
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
                    return routing.Router.GetErrorHandler(request, ResponseType.NotFound)
                                         .Handle(request)
                                         .Type(ResponseType.NotFound)
                                         .Build();
                }
            }
            catch (Exception e)
            {
                error = e;
                return null;
            }
        }

        protected IResponse ServerError(IRequest request, IRoutingContext? routing)
        {
            if (routing != null)
            {
                try
                {
                    return routing.Router.GetErrorHandler(request, ResponseType.InternalServerError)
                                         .Handle(request)
                                         .Build();
                }
                catch (Exception e)
                {
                    Server.Companion?.OnServerError(ServerErrorScope.PageGeneration, e);
                }
            }

            return CoreError(request);
        }

        protected IResponse CoreError(IRequest request)
        {
            var coreRouter = Server.Router as CoreRouter;

            if (coreRouter != null)
            {
                try
                {
                    return coreRouter.GetErrorHandler(request, ResponseType.InternalServerError)
                                     .Handle(request)
                                     .Build();
                }
                catch (Exception e)
                {
                    Server.Companion?.OnServerError(ServerErrorScope.PageGeneration, e);
                }
            }

            return GenericError(request);
        }

        protected IResponse GenericError(IRequest request)
        {
            var stream = new MemoryStream(Encoding.UTF8.GetBytes("Internal Server Error"));

            return request.Respond()
                          .Type(ResponseType.InternalServerError)
                          .Content(stream, ContentType.TextPlain)
                          .Build();
        }

        #endregion

    }

}
