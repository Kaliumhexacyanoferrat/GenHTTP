using GenHTTP.Api.Content;
using GenHTTP.Api.Content.Pages;
using GenHTTP.Api.Protocol;
using GenHTTP.Api.Routing;
using System;
using System.Collections.Generic;
using System.Text;

namespace GenHTTP.Hosting.Embedded
{

    public class ExampleRouter : IRouter
    {

        public IRouter Parent { get; set; }

        public IRoutingContext GetContext(IHttpRequest request)
        {
            return new RoutingContext(this, null);
        }

        public IContentPage GetPage(bool error)
        {
            return Parent.GetPage(error);
        }

        public IContentProvider GetProvider(ResponseType responseType, IRoutingContext context)
        {
            return Parent.GetProvider(responseType, context);
        }

    }

}
