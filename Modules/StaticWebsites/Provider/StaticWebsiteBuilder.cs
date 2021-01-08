﻿using System.Collections.Generic;

using GenHTTP.Api.Content;
using GenHTTP.Api.Content.IO;
using GenHTTP.Api.Infrastructure;

using GenHTTP.Modules.Robots;
using GenHTTP.Modules.Robots.Provider;
using GenHTTP.Modules.Sitemaps.Provider;

namespace GenHTTP.Modules.StaticWebsites.Provider
{

    public class StaticWebsiteBuilder : IHandlerBuilder<StaticWebsiteBuilder>
    {
        private IResourceTree? _Tree;

        private readonly List<IConcernBuilder> _Concerns = new();

        private SitemapProviderBuilder? _Sitemap = Sitemaps.Sitemap.Create();

        private RobotsProviderBuilder? _Robots = BotInstructions.Default().Sitemap();

        #region Functionality

        public StaticWebsiteBuilder Tree(IResourceTree tree)
        {
            _Tree = tree;
            return this;
        }

        public StaticWebsiteBuilder Sitemap(SitemapProviderBuilder? sitemap)
        {
            _Sitemap = sitemap;
            return this;
        }

        public StaticWebsiteBuilder Robots(RobotsProviderBuilder? robots)
        {
            _Robots = robots;
            return this;
        }

        public StaticWebsiteBuilder Add(IConcernBuilder concern)
        {
            _Concerns.Add(concern);
            return this;
        }

        public IHandler Build(IHandler parent)
        {
            var tree = _Tree ?? throw new BuilderMissingPropertyException("tree");

            return Concerns.Chain(parent, _Concerns, (p) => new StaticWebsiteHandler(p, tree, _Sitemap, _Robots));
        }

        #endregion

    }

}
