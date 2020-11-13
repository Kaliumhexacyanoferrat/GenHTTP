using System.Collections.Generic;
using System.Threading.Tasks;

using GenHTTP.Api.Content;
using GenHTTP.Api.Protocol;

using GenHTTP.Modules.Basics;

namespace GenHTTP.Modules.Sitemaps.Provider
{

    public class SitemapProvider : IHandler
    {
        private ContentInfo? _Info;

        #region Get-/Setters

        public IHandler Parent { get; }

        private ContentInfo Info
        {
            get
            {
                return _Info ??= ContentInfo.Create()
                                            .Title("Sitemap")
                                            .Build();
            }
        }

        #endregion

        #region Initialization

        public SitemapProvider(IHandler parent)
        {
            Parent = parent;
        }

        public ValueTask<IResponse?> HandleAsync(IRequest request)
        {
            var baseUri = $"{request.Client.Protocol?.ToString().ToLower() ?? "http"}://{request.Host}";

            var elements = new List<ContentElement>();

            foreach (var element in Parent.GetContent(request))
            {
                Flatten(element, elements);
            }

            var response = request.Respond()
                                  .Content(new SitemapContent(baseUri, elements))
                                  .Type(ContentType.TextXml)
                                  .Build();

            return new ValueTask<IResponse?>(response);
        }

        private void Flatten(ContentElement item, List<ContentElement> into)
        {
            if (item.ContentType.KnownType == ContentType.TextHtml)
            {
                into.Add(item);
            }

            if (item.Children is not null)
            {
                foreach (var child in item.Children)
                {
                    Flatten(child, into);
                }
            }
        }

        public IEnumerable<ContentElement> GetContent(IRequest request) => this.GetContent(request, Info, ContentType.TextXml);

        #endregion

    }

}
