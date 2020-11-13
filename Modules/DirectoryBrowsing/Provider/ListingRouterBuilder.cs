﻿using System.Collections.Generic;

using GenHTTP.Api.Content;
using GenHTTP.Api.Content.IO;

using GenHTTP.Api.Infrastructure;

namespace GenHTTP.Modules.DirectoryBrowsing.Provider
{

    public class ListingRouterBuilder : IHandlerBuilder<ListingRouterBuilder>
    {
        private IResourceTree? _Tree;

        private readonly List<IConcernBuilder> _Concerns = new();

        #region Functionality

        public ListingRouterBuilder Tree(IResourceTree tree)
        {
            _Tree = tree;
            return this;
        }

        public ListingRouterBuilder Tree(IBuilder<IResourceTree> tree) => Tree(tree.Build());

        public ListingRouterBuilder Add(IConcernBuilder concern)
        {
            _Concerns.Add(concern);
            return this;
        }

        public IHandler Build(IHandler parent)
        {
            var tree = _Tree ?? throw new BuilderMissingPropertyException("tree");

            return Concerns.Chain(parent, _Concerns, (p) => new ListingRouter(p, tree));
        }

        #endregion

    }

}
