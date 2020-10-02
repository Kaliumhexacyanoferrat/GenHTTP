using System.Collections.Generic;

using GenHTTP.Api.Content;
using GenHTTP.Api.Protocol;

using GenHTTP.Modules.Basics;

namespace GenHTTP.Modules.Robots.Provider
{

    public class RobotsProvider : IHandler
    {
        private ContentInfo? _Info;

        #region Get-/Setters

        private List<RobotsDirective> Directives { get; }

        private string? Sitemap { get; }

        public IHandler Parent { get; }

        private ContentInfo Info
        {
            get
            {
                return _Info ??= ContentInfo.Create()
                                            .Title("Robots Instruction File")
                                            .Build();
            }
        }

        #endregion

        #region Initialization

        public RobotsProvider(IHandler parent, List<RobotsDirective> directives, string? sitemap)
        {
            Parent = parent;

            Directives = directives;
            Sitemap = sitemap;
        }

        #endregion

        #region Functionality

        public IResponse? Handle(IRequest request)
        {
            string? sitemapUrl = null;

            if (Sitemap != null)
            {
                if (!Sitemap.StartsWith("http"))
                {
                    var protocol = request.Client.Protocol.ToString().ToLower();

                    var normalized = Sitemap.StartsWith("/") ? Sitemap.Substring(1) : Sitemap;

                    sitemapUrl = $"{protocol}://{request.Host}/{normalized}";
                }
                else
                {
                    sitemapUrl = Sitemap;
                }
            }

            return request.Respond()
                          .Type(ContentType.TextPlain)
                          .Content(new RobotsContent(Directives, sitemapUrl))
                          .Build();
        }

        public IEnumerable<ContentElement> GetContent(IRequest request) => this.GetContent(request, Info, ContentType.TextPlain);

        #endregion

    }

}
