using System;
using System.Collections.Generic;
using System.Text;

using GenHTTP.Api.Content;
using GenHTTP.Api.Protocol;

namespace GenHTTP.Api.Routing
{

    public interface IRoutingContext
    {
        
        IRouter Router { get; }

        IHttpRequest Request { get; }
        
        string ScopedPath { get; }

        IContentProvider? ContentProvider { get; }

        string? Route(string route);
        
    }

}
