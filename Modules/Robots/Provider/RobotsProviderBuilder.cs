using System.Collections.Generic;
using System.Linq;

using GenHTTP.Api.Content;

namespace GenHTTP.Modules.Robots.Provider
{

    public sealed class RobotsProviderBuilder : IHandlerBuilder<RobotsProviderBuilder>
    {
        private readonly List<RobotsDirective> _Directives = new();

        private readonly List<IConcernBuilder> _Concerns = new();

        private string? _Sitemap;

        #region Functionality

        public RobotsProviderBuilder Sitemap() => Sitemap("/" + Sitemaps.Sitemap.FILE_NAME);

        public RobotsProviderBuilder Sitemap(string url)
        {
            _Sitemap = url;
            return this;
        }

        public RobotsProviderBuilder Allow(string userAgent = "*", string path = "/")
        {
            return Directive(new string[] { userAgent }, new string[] { path }, null);
        }

        public RobotsProviderBuilder Disallow(string userAgent = "*", string path = "/")
        {
            return Directive(new string[] { userAgent }, null, new string[] { path });
        }

        public RobotsProviderBuilder Directive(IEnumerable<string>? agents, IEnumerable<string>? allowed, IEnumerable<string>? disallowed)
        {
            var emptyList = new List<string>();

            _Directives.Add(new RobotsDirective(agents?.ToList() ?? emptyList, allowed?.ToList() ?? emptyList, disallowed?.ToList() ?? emptyList));

            return this;
        }

        public RobotsProviderBuilder Add(IConcernBuilder concern)
        {
            _Concerns.Add(concern);
            return this;
        }

        public IHandler Build(IHandler parent)
        {
            return Concerns.Chain(parent, _Concerns, (p) => new RobotsProvider(p, _Directives, _Sitemap));
        }

        #endregion

    }

}
