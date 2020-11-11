using GenHTTP.Engine;

using GenHTTP.Modules.IO;

namespace Playground
{

    public static class Program
    {

        public static int Main(string[] args)
        {
            return Host.Create()
                       .Handler(Content.From(Resource.FromString("Hello World")))
                       .Run();
        }

    }

}
