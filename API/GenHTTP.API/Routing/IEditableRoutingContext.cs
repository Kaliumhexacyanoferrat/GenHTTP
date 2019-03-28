using System;
using System.Collections.Generic;
using System.Text;

using GenHTTP.Api.Modules;

namespace GenHTTP.Api.Routing
{

    public interface IEditableRoutingContext : IRoutingContext
    {
        
        void Scope(IRouter router, string? segment = null);
                
        void RegisterContent(IContentProvider contentProvider);

    }

}
