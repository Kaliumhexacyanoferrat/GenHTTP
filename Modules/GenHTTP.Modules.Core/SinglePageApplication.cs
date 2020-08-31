using GenHTTP.Modules.Core.SinglePage;

namespace GenHTTP.Modules.Core
{

    public static class SinglePageApplication
    {

        public static SinglePageBuilder From(string directory) => new SinglePageBuilder().Directory(directory);

    }

}
