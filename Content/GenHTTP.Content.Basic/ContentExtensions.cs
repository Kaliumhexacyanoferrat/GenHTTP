using System;
using System.Collections.Generic;
using System.Text;

using GenHTTP.Api.Protocol;

namespace GenHTTP.Content.Basic
{

    public static class ContentExtensions
    {
        private static readonly Dictionary<string, ContentType> CONTENT_TYPES = new Dictionary<string, ContentType>()
        {
            { "png", ContentType.ImagePng }, { "jpg", ContentType.ImageJpg }
            // ToDo
        };

        public static ContentType GuessContentType(this string fileName)
        {
            var index = fileName.LastIndexOf('.');

            if (index > -1)
            {
                var extension = fileName.Substring(index + 1).ToLowerInvariant();

                if (CONTENT_TYPES.ContainsKey(extension))
                {
                    return CONTENT_TYPES[extension];
                }
            }

            return ContentType.ApplicationForceDownload;
        }

    }

}
