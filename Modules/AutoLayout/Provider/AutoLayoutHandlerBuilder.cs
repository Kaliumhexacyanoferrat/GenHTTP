using GenHTTP.Api.Content;
using GenHTTP.Api.Content.IO;
using GenHTTP.Api.Infrastructure;
using GenHTTP.Modules.AutoLayout.Scanning;
using System.Collections.Generic;

namespace GenHTTP.Modules.AutoLayout.Provider
{

    public class AutoLayoutHandlerBuilder : IHandlerBuilder<AutoLayoutHandlerBuilder>
    {
        private readonly List<IConcernBuilder> _Concerns = new();

        private IResourceTree? _Tree;

        private HandlerRegistryBuilder? _Registry;

        private string[]? _Index;

        #region Functionality

        public AutoLayoutHandlerBuilder Tree(IResourceTree tree)
        {
            _Tree = tree;
            return this;
        }

        public AutoLayoutHandlerBuilder Registry(HandlerRegistryBuilder registry)
        {
            _Registry = registry;
            return this;
        }

        public AutoLayoutHandlerBuilder Index(params string[] index)
        {
            _Index = index;
            return this;
        }

        public AutoLayoutHandlerBuilder Add(IConcernBuilder concern)
        {
            _Concerns.Add(concern);
            return this;
        }

        public IHandler Build(IHandler parent)
        {
            var tree = _Tree ?? throw new BuilderMissingPropertyException("tree");

            var registry = _Registry ?? Resolvers.Default();

            var index = _Index ?? new[] { "Index" };

            return Concerns.Chain(parent, _Concerns, (p) => new AutoLayoutHandler(p, tree, registry.Build(), index));
        }

        #endregion

    }

}
