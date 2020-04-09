﻿using System;

using GenHTTP.Api.Protocol;
using GenHTTP.Api.Content.Templating;

namespace GenHTTP.Modules.Core.Templating
{

    public static class TemplatingExtensions
    {

        public static IResponseBuilder Content(this IResponseBuilder response, TemplateModel model)
        {
            /*if (response.Request.Routing == null)
            {
                throw new InvalidOperationException("Routing context is not available");
            }

            var content = response.Request.Routing.Router
                                          .GetRenderer()
                                          .Render(model);*/
            
            return response.Content("ToDo") // ToDo
                           .Type(ContentType.TextHtml);
        }

    }

}
