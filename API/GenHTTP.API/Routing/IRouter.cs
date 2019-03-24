using System;
using System.Collections.Generic;
using System.Text;

using GenHTTP.Api.Content;
using GenHTTP.Api.Content.Pages;
using GenHTTP.Api.Protocol;

namespace GenHTTP.Api.Routing
{

    public interface IRouter
    {

        IRouter Parent { get; set; }

        IRoutingContext GetContext(IHttpRequest request);

        IContentProvider GetProvider(ResponseType responseType, IRoutingContext context);

        IContentPage GetPage(bool error);

    }

}
