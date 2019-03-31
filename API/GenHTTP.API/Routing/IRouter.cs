using System;
using System.Collections.Generic;
using System.Text;

using GenHTTP.Api.Modules;
using GenHTTP.Api.Modules.Templating;
using GenHTTP.Api.Protocol;

namespace GenHTTP.Api.Routing
{

    public interface IRouter
    {

        IRouter Parent { get; set; }

        void HandleContext(IEditableRoutingContext current);
        
        IRenderer<TemplateModel> GetRenderer();

        IContentProvider GetErrorHandler(IRequest request, ResponseType responseType);

        string? Route(string path, int currentDepth);

    }

}
