using GenHTTP.Api.Routing;
using GenHTTP.Modules.Core;
using GenHTTP.Modules.Webservices;

namespace GenHTTP.Examples.Examples.Webservices
{

    public static class WebserviceExamples
    {

        public static IRouterBuilder Create()
        {
            return Layout.Create()
                         .Add<BookResource>("books");
        }

    }

}
