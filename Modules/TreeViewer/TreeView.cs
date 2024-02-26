using GenHTTP.Api.Content.IO;
using GenHTTP.Api.Infrastructure;

using GenHTTP.Modules.TreeViewer.Provider;

namespace GenHTTP.Modules.TreeViewer
{
    public static class TreeView
    {

        public static TreeViewHandlerBuilder From(IResourceTree tree) => new TreeViewHandlerBuilder().Tree(tree);

        public static TreeViewHandlerBuilder From(IBuilder<IResourceTree> tree) => new TreeViewHandlerBuilder().Tree(tree.Build());

    }

}
