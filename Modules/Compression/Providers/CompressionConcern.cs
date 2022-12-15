using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading.Tasks;

using GenHTTP.Api.Content;
using GenHTTP.Api.Infrastructure;
using GenHTTP.Api.Protocol;
using GenHTTP.Api.Routing;

namespace GenHTTP.Modules.Compression.Providers
{

    public sealed class CompressionConcern : IConcern
    {
        private const string ACCEPT_ENCODING = "Accept-Encoding";

        private const string VARY = "Vary";

        #region Get-/Setters

        public IHandler Content { get; }

        public IHandler Parent { get; }

        private IReadOnlyDictionary<string, ICompressionAlgorithm> Algorithms { get; }

        private CompressionLevel Level { get; }

        #endregion

        #region Initialization

        public CompressionConcern(IHandler parent, Func<IHandler, IHandler> contentFactory, 
                                  IReadOnlyDictionary<string, ICompressionAlgorithm> algorithms,
                                  CompressionLevel level)
        {
            Parent = parent;
            Content = contentFactory(this);

            Algorithms = algorithms;
            Level = level;
        }

        #endregion

        #region Functionality

        public async ValueTask<IResponse?> HandleAsync(IRequest request)
        {
            var response = await Content.HandleAsync(request).ConfigureAwait(false);

            if (response is not null)
            {
                if (response.ContentEncoding is null)
                {
                    if (response.Content is not null && ShouldCompress(request.Target.Path, response.ContentType?.KnownType))
                    {
                        if (request.Headers.TryGetValue(ACCEPT_ENCODING, out var header))
                        {
                            if (!string.IsNullOrEmpty(header))
                            {
                                var supported = new HashSet<string>(header.Split(",".ToCharArray(), StringSplitOptions.RemoveEmptyEntries).Select(a => a.Trim()));

                                foreach (var algorithm in Algorithms.Values.OrderByDescending(a => (int)a.Priority))
                                {
                                    if (supported.Contains(algorithm.Name))
                                    {
                                        response.Content = algorithm.Compress(response.Content, Level);
                                        response.ContentEncoding = algorithm.Name;
                                        response.ContentLength = null;

                                        if (response.Headers.TryGetValue(VARY, out var existing))
                                        {
                                            response.Headers[VARY] = $"{existing}, {ACCEPT_ENCODING}";
                                        }
                                        else
                                        {
                                            response.Headers[VARY] = ACCEPT_ENCODING;
                                        }

                                        break;
                                    }
                                }
                            }
                        }
                    }
                }
            }

            return response;
        }

        private static bool ShouldCompress(WebPath path, ContentType? type)
        {
            if (type is not null)
            {
                switch (type)
                {
                    case ContentType.ApplicationJavaScript:
                    case ContentType.ApplicationJson:
                    case ContentType.AudioWav:
                    case ContentType.TextCss:
                    case ContentType.TextCsv:
                    case ContentType.TextHtml:
                    case ContentType.TextPlain:
                    case ContentType.TextRichText:
                    case ContentType.FontWoff2:
                    case ContentType.FontWoff:
                    case ContentType.FontTrueTypeFont:
                    case ContentType.FontOpenTypeFont:
                    case ContentType.FontEmbeddedOpenTypeFont:
                    case ContentType.ImageScalableVectorGraphics:
                    case ContentType.ImageScalableVectorGraphicsXml:
                    case ContentType.ImageBmp:
                    case ContentType.TextXml:
                    case ContentType.TextJavaScript:
                        {
                            return true;
                        }
                }
            }

            if (path.File is not null)
            {
                switch (Path.GetExtension(path.File))
                {
                    case ".rrd": return true;
                }
            }

            return false;
        }

        public ValueTask PrepareAsync() => Content.PrepareAsync();

        public IAsyncEnumerable<ContentElement> GetContentAsync(IRequest request) => Content.GetContentAsync(request);

        #endregion

    }

}
