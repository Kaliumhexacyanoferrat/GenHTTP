using System;
using System.Collections.Generic;
using System.Text;

using GenHTTP.Api.Content;

namespace GenHTTP.Api.Routing
{

    public interface IRoutingContext
    {
        
        IRouter Router { get; }
        
        IContentProvider? ContentProvider { get; }
        
    }

}
