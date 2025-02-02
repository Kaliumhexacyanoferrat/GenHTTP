﻿using GenHTTP.Api.Content;
using GenHTTP.Api.Content.IO;
using GenHTTP.Api.Infrastructure;

namespace GenHTTP.Modules.IO.Providers;

public sealed class ResourceHandlerBuilder : IHandlerBuilder<ResourceHandlerBuilder>
{
    private readonly List<IConcernBuilder> _Concerns = [];

    private IResourceTree? _Tree;

    #region Functionality

    public ResourceHandlerBuilder Tree(IBuilder<IResourceTree> tree) => Tree(tree.Build());

    public ResourceHandlerBuilder Tree(IResourceTree tree)
    {
        _Tree = tree;
        return this;
    }

    public ResourceHandlerBuilder Add(IConcernBuilder concern)
    {
        _Concerns.Add(concern);
        return this;
    }

    public IHandler Build()
    {
        var tree = _Tree ?? throw new BuilderMissingPropertyException("tree");

        return Concerns.Chain(_Concerns,  new ResourceHandler( tree));
    }

    #endregion

}
