using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Xml;

using GenHTTP.Api.Content;
using GenHTTP.Api.Protocol;

namespace GenHTTP.Modules.Sitemaps.Provider
{

    public sealed class SitemapContent : IResponseContent
    {
        private static readonly XmlWriterSettings SETTINGS = new()
        {
            Async = true,
            CloseOutput = false
        };

        #region Get-/Setters

        public ulong? Length => null;

        private string BaseUri { get; }

        private IEnumerable<ContentElement> Elements { get; }

        #endregion

        #region Initialization

        public SitemapContent(string baseUri, IEnumerable<ContentElement> elements)
        {
            Elements = elements;
            BaseUri = baseUri;
        }

        #endregion

        #region Functionality

        public ValueTask<ulong?> CalculateChecksumAsync()
        {
            unchecked
            {
                ulong hash = 17;

                foreach (var element in Elements)
                {
                    hash = hash * 23 + (ulong)element.Path.ToString().GetHashCode();
                }

                return new ValueTask<ulong?>(hash);
            }
        }

        public async ValueTask WriteAsync(Stream target, uint bufferSize)
        {
            using (var writer = XmlWriter.Create(target, SETTINGS))
            {
                await writer.WriteStartDocumentAsync();

                {
                    await writer.WriteStartElementAsync(null, "urlset", "http://www.sitemaps.org/schemas/sitemap/0.9");

                    foreach (var element in Elements)
                    {
                        await writer.WriteStartElementAsync(null, "url", null);

                        await writer.WriteElementStringAsync(null, "loc", null, BaseUri + element.Path.ToString());

                        await writer.WriteEndElementAsync();
                    }

                    await writer.WriteEndElementAsync();
                }

                await writer.WriteEndDocumentAsync();
            }
        }

        #endregion

    }

}
