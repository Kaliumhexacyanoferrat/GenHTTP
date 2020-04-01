using System.Collections.Generic;

using GenHTTP.Api.Modules;
using GenHTTP.Api.Protocol;
using GenHTTP.Modules.Core.General;

namespace GenHTTP.Modules.Core.Bots
{

    public class RobotsProvider : ContentProviderBase
    {

        #region Get-/Setters

        public override string? Title => "Robots Instruction File";

        public override FlexibleContentType? ContentType => new FlexibleContentType(Api.Protocol.ContentType.TextPlain);

        protected override HashSet<FlexibleRequestMethod>? SupportedMethods => _GET;

        private List<RobotsDirective> Directives { get; }

        private string? Sitemap { get; }

        #endregion

        #region Initialization

        public RobotsProvider(List<RobotsDirective> directives, string? sitemap, ResponseModification? modification) : base(modification)
        {
            Directives = directives;
            Sitemap = sitemap;
        }

        #endregion

        #region Functionality

        protected override IResponseBuilder HandleInternal(IRequest request)
        {
            string? sitemapUrl = null;

            if (Sitemap != null)
            {
                if (!Sitemap.StartsWith("http"))
                {
                    var protocol = request.Client.Protocol.ToString().ToLower();

                    var normalized = (Sitemap.StartsWith("/")) ? Sitemap.Substring(1) : Sitemap;

                    sitemapUrl = $"{protocol}://{request.Host}/{normalized}";
                }
                else
                {
                    sitemapUrl = Sitemap;
                }
            }

            return request.Respond()
                          .Type(Api.Protocol.ContentType.TextPlain)
                          .Content(new RobotsContent(Directives, sitemapUrl));
        }

        #endregion

    }

}
