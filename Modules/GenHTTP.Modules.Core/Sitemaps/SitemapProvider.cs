using System.Collections.Generic;

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

        protected override HashSet<FlexibleRequestMethod>? SupportedMethods => _GET;

        #endregion

        #region Initialization

        public SitemapProvider(IRouter content, ResponseModification? modification) : base(modification)
        {
            Content = content;
        }

        protected override IResponseBuilder HandleInternal(IRequest request)
        {
            var baseUri = $"{request.Client.Protocol.ToString().ToLower()}://{request.Host}/";

            var elements = new List<ContentElement>();

            foreach (var element in Content.GetContent(request, baseUri))
            {
                Flatten(element, elements);
            }

            return request.Respond()
                          .Content(new SitemapContent(elements))
                          .Type(Api.Protocol.ContentType.TextXml);
        }

        private void Flatten(ContentElement item, List<ContentElement> into)
        {
            if (item.ContentType?.KnownType == Api.Protocol.ContentType.TextHtml)
            {
                into.Add(item);
            }

            if (item.Children != null)
            {
                foreach (var child in item.Children)
                {
                    Flatten(child, into);
                }
            }
        }

        #endregion

    }

}
