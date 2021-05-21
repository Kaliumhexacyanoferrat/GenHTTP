using GenHTTP.Api.Content.Templating;
using GenHTTP.Api.Routing;

using GenHTTP.Modules.Basics;

namespace GenHTTP.Modules.Razor
{

    public static class RazorExtensions
    {

        public static string? Route(this IModel model, string target)
        {
            return model.Handler.Route(model.Request, target);
        }

        public static string? Route(this IModel model, WebPath target)
        {
            return model.Handler.Route(model.Request, target);
        }

    }

}
