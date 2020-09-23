using GenHTTP.Modules.SinglePageApplications.Provider;

namespace GenHTTP.Modules.SinglePageApplications
{

    public static class SinglePageApplication
    {

        public static SinglePageBuilder From(string directory) => new SinglePageBuilder().Directory(directory);

    }

}
