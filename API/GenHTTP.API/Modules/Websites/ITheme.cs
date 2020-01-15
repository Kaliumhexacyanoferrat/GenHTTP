using System;
using System.Collections.Generic;

using GenHTTP.Api.Modules.Templating;
using GenHTTP.Api.Protocol;
using GenHTTP.Api.Routing;

namespace GenHTTP.Api.Modules.Websites
{

    public interface ITheme
    {

        List<Script> Scripts { get; }

        List<Style> Styles { get; }

        IRouter? Resources { get; }

        IContentProvider? GetErrorHandler(IRequest request, ResponseStatus responseType, Exception? cause);

        IRenderer<WebsiteModel> GetRenderer();

        object? GetModel(IRequest request);

    }

}
