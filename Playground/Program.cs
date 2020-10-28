using GenHTTP.Engine;

using GenHTTP.Modules.IO;
using GenHTTP.Modules.Practices;

namespace Playground
{

    public static class Program
    {

        public static int Main(string[] args)
        {
            var handler = Content.FromString("Hello World");

            return Host.Create()
                       .Handler(handler)
                       .Run();
        }

    }

}
