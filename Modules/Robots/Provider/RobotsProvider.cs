using System.Collections.Generic;
using System.Threading.Tasks;

using GenHTTP.Api.Content;
using GenHTTP.Api.Protocol;

using GenHTTP.Modules.Basics;

namespace GenHTTP.Modules.Robots.Provider
{

    public sealed class RobotsProvider : IHandler
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

        public ValueTask<IResponse?> HandleAsync(IRequest request)
        {
            string? sitemapUrl = null;

            if (Sitemap is not null)
            {
                if (!Sitemap.StartsWith("http"))
                {
                    var protocol = request.Client.Protocol?.ToString().ToLower() ?? "http";

                    var normalized = Sitemap.StartsWith("/") ? Sitemap[1..] : Sitemap;

                    sitemapUrl = $"{protocol}://{request.Host}/{normalized}";
                }
                else
                {
                    sitemapUrl = Sitemap;
                }
            }

            var response = request.Respond()
                                  .Type(ContentType.TextPlain)
                                  .Content(new RobotsContent(Directives, sitemapUrl))
                                  .Build();

            return new ValueTask<IResponse?>(response);
        }

        public ValueTask PrepareAsync() => ValueTask.CompletedTask;

        public IAsyncEnumerable<ContentElement> GetContentAsync(IRequest request) => this.GetContent(request, Info, ContentType.TextPlain);

        #endregion

    }

}
