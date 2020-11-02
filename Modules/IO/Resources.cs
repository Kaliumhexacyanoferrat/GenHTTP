using GenHTTP.Api.Content.IO;
using GenHTTP.Api.Infrastructure;

using GenHTTP.Modules.IO.Providers;

namespace GenHTTP.Modules.IO
{

    
    public static class Resources
    {

        public static ResourceHandlerBuilder From(IBuilder<IResourceTree> tree) => From(tree.Build());

        public static ResourceHandlerBuilder From(IResourceTree tree) => new ResourceHandlerBuilder().Tree(tree);

    }

}
