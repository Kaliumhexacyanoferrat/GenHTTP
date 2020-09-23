using GenHTTP.Engine;

using GenHTTP.Modules.IO;
using GenHTTP.Modules.Practices;

namespace Playground
{

    public static class Program
    {

        public static int Main(string[] args)
        {
            return Host.Create()
                       .Handler(Content.From("Hello World!"))
                       .Console()
                       .Defaults()
                       .Run();
        }

    }

}
