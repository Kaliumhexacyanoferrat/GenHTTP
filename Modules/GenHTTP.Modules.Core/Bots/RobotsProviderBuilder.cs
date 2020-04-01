using System.Collections.Generic;
using System.Linq;

using GenHTTP.Api.Modules;
using GenHTTP.Modules.Core.General;

namespace GenHTTP.Modules.Core.Bots
{

    public class RobotsProviderBuilder : ContentBuilderBase
    {
        private readonly List<RobotsDirective> _Directives = new List<RobotsDirective>();

        private string? _Sitemap;

        #region Functionality

        public RobotsProviderBuilder Sitemap() => Sitemap("/sitemaps/sitemap.xml");

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

        public override IContentProvider Build()
        {
            return new RobotsProvider(_Directives, _Sitemap, _Modification);
        }

        #endregion

    }

}
