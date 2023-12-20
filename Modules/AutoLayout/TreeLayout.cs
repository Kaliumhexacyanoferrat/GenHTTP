using GenHTTP.Api.Content.IO;
using GenHTTP.Api.Infrastructure;
using GenHTTP.Modules.AutoLayout.Provider;

namespace GenHTTP.Modules.AutoLayout
{
    public static class TreeLayout
    {

        public static AutoLayoutHandlerBuilder From(IResourceTree tree) => new AutoLayoutHandlerBuilder().Tree(tree);

        public static AutoLayoutHandlerBuilder From(IBuilder<IResourceTree> tree) => new AutoLayoutHandlerBuilder().Tree(tree.Build());

    }

}
