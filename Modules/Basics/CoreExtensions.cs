using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

using GenHTTP.Api.Content.IO;
using GenHTTP.Api.Protocol;
using GenHTTP.Api.Routing;

namespace GenHTTP.Modules.Basics
{

    public static class CoreExtensions
    {

        #region Request

        public static bool HasType(this IRequest request, params RequestMethod[] methods)
        {
            foreach (var method in methods)
            {
                if (request.Method == method)
                {
                    return true;
                }
            }

            return false;
        }

        public static string? HostWithoutPort(this IRequest request)
        {
            var host = request.Host;

            if (host is not null)
            {
                var pos = host.IndexOf(':');

                if (pos > 0)
                {
                    return host.Substring(0, pos);
                }
                else
                {
                    return host;
                }
            }

            return null;
        }

        #endregion

        #region Response builder

        /// <summary>
        /// Specifies the content type of this response.
        /// </summary>
        /// <param name="contentType">The content type of this response</param>
        public static IResponseBuilder Type(this IResponseBuilder builder, ContentType contentType) => builder.Type(FlexibleContentType.Get(contentType));

        /// <summary>
        /// Specifies the content type of this response.
        /// </summary>
        /// <param name="contentType">The content type of this response</param>
        public static IResponseBuilder Type(this IResponseBuilder builder, string contentType) => builder.Type(FlexibleContentType.Parse(contentType));

        #endregion

        #region Content types

        private static readonly Dictionary<string, ContentType> CONTENT_TYPES = new() {
            // CSS
            { "css", ContentType.TextCss },
            // HTML
            { "html", ContentType.TextHtml },
            { "htm", ContentType.TextHtml },
            // Text files            
            { "txt", ContentType.TextPlain },
            { "conf", ContentType.TextPlain },
            { "config", ContentType.TextPlain },
            // Fonts 
            { "eot", ContentType.FontEmbeddedOpenTypeFont },
            { "ttf", ContentType.FontTrueTypeFont },
            { "otf", ContentType.FontOpenTypeFont },
            { "woff", ContentType.FontWoff },
            { "woff2", ContentType.FontWoff2 },
            // Scripts
            { "js", ContentType.ApplicationJavaScript },
            // Images
            { "ico", ContentType.ImageIcon },
            { "gif", ContentType.ImageGif },
            { "jpeg", ContentType.ImageJpg },
            { "jpg", ContentType.ImageJpg },
            { "png", ContentType.ImagePng },
            { "bmp", ContentType.ImageBmp },
            { "tiff", ContentType.ImageTiff },
            { "svg", ContentType.ImageScalableVectorGraphics },
            { "svgz", ContentType.ImageScalableVectorGraphicsCompressed },
            // Audio
            { "ogg", ContentType.AudioOgg },
            { "mp3", ContentType.AudioMpeg },
            { "wav", ContentType.AudioWav },
            // Video
            { "mpg", ContentType.VideoMpeg },
            { "mpeg", ContentType.VideoMpeg },
            { "avi", ContentType.VideoMpeg },
            // Documents
            { "csv", ContentType.TextCsv },
            { "rtf", ContentType.TextRichText },
            { "docx", ContentType.ApplicationOfficeDocumentWordProcessing },
            { "pptx", ContentType.ApplicationOfficeDocumentPresentation },
            { "ppsx", ContentType.ApplicationOfficeDocumentSlideshow },
            { "xslx", ContentType.ApplicationOfficeDocumentSheet },
            // Object models
            { "xml", ContentType.TextXml }
        };

        public static ContentType? GuessContentType(this string fileName)
        {
            var extension = Path.GetExtension(fileName);

            if (extension is not null && extension.Length > 1)
            {
                extension = extension[1..].ToLower();

                if (CONTENT_TYPES.TryGetValue(extension, out var value))
                {
                    return value;
                }
            }

            return null;
        }

        #endregion

        #region Resource provider

        public static async ValueTask<string> GetResourceAsStringAsync(this IResource resourceProvider)
        {
            using var stream = await resourceProvider.GetContentAsync();

            return await new StreamReader(stream).ReadToEndAsync();
        }

        #endregion

        #region Routing

        public static string RelativeTo(this WebPath path, WebPath target)
        {
            var common = CommonParts(path, target);

            var hops = path.Parts.Count - common + (path.TrailingSlash ? 1 : 0) - 1;

            var relativeParts = new List<WebPathPart>();

            if (hops > 0)
            {
                for (int i = 0; i < hops; i++)
                {
                    relativeParts.Add(new(".."));
                }
            }
            else
            {
                relativeParts.Add(new("."));
            }

            var added = false;

            for (int i = common; i < target.Parts.Count; i++)
            {
                relativeParts.Add(target.Parts[i]);
                added = true;
            }

            var trailing = target.TrailingSlash || !added;

            return new WebPath(relativeParts, trailing).ToString()[1..];
        }

        public static WebPath Combine(this WebPath path, WebPath target)
        {
            var parts = new List<WebPathPart>(path.Parts);

            if (target.Parts.Count > 0)
            {
                var index = 0;

                while (target.Parts.Count > index)
                {
                    if (target.Parts[index] == ".")
                    {
                        index++;
                    }
                    else if (target.Parts[index] == "..")
                    {
                        if (parts.Count > 0)
                        {
                            parts.RemoveAt(parts.Count - 1);
                        }
                        else
                        {
                            throw new InvalidOperationException($"Target path '{target}' does exceed the hierarchy levels of path '{path}' and cannot be combined");
                        }

                        index++;
                    }
                    else
                    {
                        break;
                    }
                }

                parts.AddRange(target.Parts.Skip(index));
            }

            return new WebPath(parts, target.TrailingSlash);
        }

        private static int CommonParts(WebPath one, WebPath two)
        {
            int common;

            for (common = 0; common < one.Parts.Count; common++)
            {
                if (common >= two.Parts.Count)
                {
                    return common;
                }

                if (!two.Parts[common].Equals(one.Parts[common]))
                {
                    return common;
                }
            }

            return common;
        }

        #endregion

    }

}
