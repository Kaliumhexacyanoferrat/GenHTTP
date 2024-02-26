using System.Collections.Generic;

using GenHTTP.Api.Content;
using GenHTTP.Api.Content.IO;
using GenHTTP.Api.Infrastructure;

namespace GenHTTP.Modules.TreeViewer.Provider
{

    public class TreeViewHandlerBuilder : IHandlerBuilder<TreeViewHandlerBuilder>
    {
        private readonly List<IConcernBuilder> _Concerns = new();

        private IResourceTree? _Tree;

        private ITreeRenderer? _Renderer;

        #region Functionality

        public TreeViewHandlerBuilder Tree(IResourceTree tree)
        {
            _Tree = tree;
            return this;
        }

        public TreeViewHandlerBuilder Renderer(ITreeRenderer renderer)
        {
            _Renderer = renderer;
            return this;
        }

        public TreeViewHandlerBuilder Add(IConcernBuilder concern)
        {
            _Concerns.Add(concern);
            return this;
        }

        public IHandler Build(IHandler parent)
        {
            var tree = _Tree ?? throw new BuilderMissingPropertyException("tree");

            var renderer = _Renderer ?? throw new BuilderMissingPropertyException("renderer");

            return Concerns.Chain(parent, _Concerns, (p) => new TreeViewHandler(p, tree, renderer));
        }

        #endregion

    }

}
