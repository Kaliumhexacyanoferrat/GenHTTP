using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

using GenHTTP.Api.Content;
using GenHTTP.Api.Infrastructure;
using GenHTTP.Api.Protocol;
using GenHTTP.Api.Routing;

namespace GenHTTP.Modules.Compression.Providers
{

    public class CompressionConcern : IConcern
    {

        #region Get-/Setters

        public IHandler Content { get; }

        private IReadOnlyDictionary<string, ICompressionAlgorithm> Algorithms { get; }

        public IHandler Parent { get; }

        #endregion

        #region Initialization

        public CompressionConcern(IHandler parent, Func<IHandler, IHandler> contentFactory, IReadOnlyDictionary<string, ICompressionAlgorithm> algorithms)
        {
            Parent = parent;
            Content = contentFactory(this);

            Algorithms = algorithms;
        }

        #endregion

        #region Functionality

        public async ValueTask<IResponse?> HandleAsync(IRequest request)
        {
            var response = await Content.HandleAsync(request).ConfigureAwait(false);

            if (response != null)
            {
                if (response.ContentEncoding == null)
                {
                    if (response.Content != null && ShouldCompress(request.Target.Path, response.ContentType?.KnownType))
                    {
                        if (request.Headers.TryGetValue("Accept-Encoding", out var header))
                        {
                            if (!string.IsNullOrEmpty(header))
                            {
                                var supported = new HashSet<string>(header.Split(",".ToCharArray(), StringSplitOptions.RemoveEmptyEntries).Select(a => a.Trim()));

                                foreach (var algorithm in Algorithms.Values.OrderByDescending(a => (int)a.Priority))
                                {
                                    if (supported.Contains(algorithm.Name))
                                    {
                                        response.Content = algorithm.Compress(response.Content);
                                        response.ContentEncoding = algorithm.Name;
                                        response.ContentLength = null;

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

        private bool ShouldCompress(WebPath path, ContentType? type)
        {
            if (type != null)
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
                    case ContentType.ImageBmp:
                    case ContentType.TextXml:
                        {
                            return true;
                        }
                }
            }

            if (path.File != null)
            {
                switch (Path.GetExtension(path.File))
                {
                    case ".rrd": return true;
                }
            }

            return false;
        }

        public IEnumerable<ContentElement> GetContent(IRequest request)
        {
            return Content.GetContent(request);
        }

        #endregion

    }

}
