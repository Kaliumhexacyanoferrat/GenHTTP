using System;

using GenHTTP.Api.Protocol;

namespace GenHTTP.Modules.Minification.Plugins.Html
{

    public sealed class HtmlPlugin : IMinificationPlugin
    {

        public bool Supports(IResponse response)
        {
            var contentType = response.ContentType?.KnownType;

            return (contentType == ContentType.TextHtml);
        }

        public void Process(IResponse response, MinificationErrors errorHandling)
        {
            response.Content = new MinifiedHtml(response.Content ?? throw new InvalidOperationException("Response has not content to be minified"), errorHandling);
            response.ContentLength = null;
        }

    }

}
