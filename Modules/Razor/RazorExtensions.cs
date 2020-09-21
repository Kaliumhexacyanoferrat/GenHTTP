﻿using GenHTTP.Api.Content.Templating;

using GenHTTP.Modules.Basics;

namespace GenHTTP.Modules.Razor
{

    public static class RazorExtensions
    {

        public static string? Route(this IBaseModel model, string target)
        {
            return model.Handler.Route(model.Request, target);
        }

    }

}
