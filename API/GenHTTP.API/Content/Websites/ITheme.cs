using System;
using System.Collections.Generic;

using GenHTTP.Api.Content.Templating;
using GenHTTP.Api.Protocol;

namespace GenHTTP.Api.Content.Websites
{

    public interface ITheme
    {

        List<Script> Scripts { get; }

        List<Style> Styles { get; }

        IHandlerBuilder? Resources { get; }

        IRenderer<WebsiteModel> GetRenderer();

        object? GetModel(IRequest request);

    }

}
