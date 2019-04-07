using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Linq;

using GenHTTP.Api.Infrastructure;
using GenHTTP.Api.Protocol;

namespace GenHTTP.Modules.Core.Compression
{

    public class CompressionExtension : IServerExtension
    {

        #region Get-/Setters

        private IReadOnlyDictionary<string, ICompressionAlgorithm> Algorithms { get; }

        #endregion

        #region Initialization

        public CompressionExtension(IReadOnlyDictionary<string, ICompressionAlgorithm> algorithms)
        {
            Algorithms = algorithms;
        }

        #endregion

        #region Functionality

        public void Intercept(IRequest request, IResponse response)
        {
            if (response.ContentEncoding == null)
            {
                if ((response.Content != null) && ShouldCompress(response.ContentType?.KnownType))
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
        
        private bool ShouldCompress(ContentType? type)
        {
            if (type != null)
            {
                switch (type)
                {
                    case ContentType.ApplicationJavaScript:
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

            return false;
        }

        #endregion

    }

}
