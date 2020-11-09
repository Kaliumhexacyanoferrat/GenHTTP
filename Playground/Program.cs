using GenHTTP.Engine;

using GenHTTP.Modules.IO;
using GenHTTP.Modules.Practices;
using GenHTTP.Modules.Webservices;

namespace Playground
{

    public static class Program
    {

        public static int Main(string[] args)
        {
            var handler = GenHTTP.Modules.Webservices.ServiceResource.From<JsonResource>();

            return Host.Create()
                       .Handler(handler)
                       .Run();
        }

    }

    public class JsonResult
    {

        public string Message { get; set; }

    }

    public class JsonResource
    {

        [ResourceMethod]
        public JsonResult GetMessage() => new JsonResult() { Message = "Hello, World!" };

    }

}
