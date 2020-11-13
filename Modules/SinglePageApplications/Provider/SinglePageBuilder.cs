using System.Collections.Generic;

using GenHTTP.Api.Content;
using GenHTTP.Api.Content.IO;
using GenHTTP.Api.Infrastructure;

namespace GenHTTP.Modules.SinglePageApplications.Provider
{

    public class SinglePageBuilder : IHandlerBuilder<SinglePageBuilder>
    {
        private IResourceTree? _Tree;

        private readonly List<IConcernBuilder> _Concerns = new();

        #region Functionality

        public SinglePageBuilder Add(IConcernBuilder concern)
        {
            _Concerns.Add(concern);
            return this;
        }

        public SinglePageBuilder Tree(IResourceTree tree)
        {
            _Tree = tree;
            return this;
        }

        public SinglePageBuilder Tree(IBuilder<IResourceTree> tree) => Tree(tree.Build());

        public IHandler Build(IHandler parent)
        {
            var tree = _Tree ?? throw new BuilderMissingPropertyException("tree");

            return Concerns.Chain(parent, _Concerns, (p) => new SinglePageProvider(p, tree));
        }

        #endregion

    }

}
