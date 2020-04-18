using GenHTTP.Api.Content;
using GenHTTP.Modules.Core;
using GenHTTP.Modules.Webservices;

namespace GenHTTP.Examples.Examples.Webservices
{

    public static class WebserviceExamples
    {

        public static IHandlerBuilder Create()
        {
            return Layout.Create()
                         .Add<BookResource>("books");
        }

    }

}
