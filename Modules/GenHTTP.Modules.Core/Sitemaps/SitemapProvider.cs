using System.Linq;

using GenHTTP.Api.Modules;
using GenHTTP.Api.Protocol;
using GenHTTP.Api.Routing;

using GenHTTP.Modules.Core.General;

namespace GenHTTP.Modules.Core.Sitemaps
{

    public class SitemapProvider : ContentProviderBase
    {

        #region Get-/Setters

        private IRouter Content { get; }

        public override string? Title => "Sitemap";

        public override FlexibleContentType? ContentType => new FlexibleContentType(Api.Protocol.ContentType.TextXml);

        #endregion

        #region Initialization

        public SitemapProvider(IRouter content, ResponseModification? modification) : base(modification)
        {
            Content = content;
        }

        protected override IResponseBuilder HandleInternal(IRequest request)
        {
            var baseUri = $"{request.Client.Protocol.ToString().ToLower()}://{request.Host}/";

            var elements = Content.GetContent(request, baseUri)
                                  .Where(e => e.ContentType?.KnownType == Api.Protocol.ContentType.TextHtml);

            return request.Respond()
                          .Content(new SitemapContent(elements))
                          .Type(Api.Protocol.ContentType.TextXml);
        }

        #endregion

    }

}
