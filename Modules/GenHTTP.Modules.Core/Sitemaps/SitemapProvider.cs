using System.Collections.Generic;

using GenHTTP.Api.Content;
using GenHTTP.Api.Protocol;

namespace GenHTTP.Modules.Core.Sitemaps
{

    public class SitemapProvider : IHandler
    {

        #region Get-/Setters

        public IHandler Parent { get; }

        #endregion

        #region Initialization

        public SitemapProvider(IHandler parent)
        {
            Parent = parent;
        }

        public IResponse Handle(IRequest request)
        {
            var baseUri = $"{request.Client.Protocol.ToString().ToLower()}://{request.Host}";

            var elements = new List<ContentElement>();

            foreach (var element in Parent.GetContent(request))
            {
                Flatten(element, elements);
            }

            return request.Respond()
                          .Content(new SitemapContent(baseUri, elements))
                          .Type(ContentType.TextXml)
                          .Build();
        }

        private void Flatten(ContentElement item, List<ContentElement> into)
        {
            if (item.ContentType?.KnownType == ContentType.TextHtml)
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

        public IEnumerable<ContentElement> GetContent(IRequest request) => this.GetContent(request, "Sitemap", ContentType.TextXml);

        #endregion

    }

}
