using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Xml;

using GenHTTP.Api.Modules;
using GenHTTP.Api.Protocol;

namespace GenHTTP.Modules.Core.Sitemaps
{

    public class SitemapContent : IResponseContent
    {
        private static readonly XmlWriterSettings SETTINGS = new XmlWriterSettings()
        {
            Async = true,
            CloseOutput = false
        };

        #region Get-/Setters

        public ulong? Length => null;

        private IEnumerable<ContentElement> Elements { get; }

        #endregion

        #region Initialization

        public SitemapContent(IEnumerable<ContentElement> elements)
        {
            Elements = elements;
        }

        #endregion

        #region Functionality

        public async Task Write(Stream target, uint bufferSize)
        {
            using (var writer = XmlWriter.Create(target, SETTINGS))
            {
                await writer.WriteStartDocumentAsync();

                {
                    await writer.WriteStartElementAsync(null, "urlset", "http://www.sitemaps.org/schemas/sitemap/0.9");

                    foreach (var element in Elements)
                    {
                        await writer.WriteStartElementAsync(null, "url", null);

                        await writer.WriteElementStringAsync(null, "loc", null, element.Path);

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
