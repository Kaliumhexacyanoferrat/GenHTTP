using System;

using GenHTTP.Api.Protocol;

namespace GenHTTP.Modules.Minification.Plugins.CSS
{

    public sealed class CssPlugin : IMinificationPlugin
    {

        public bool Supports(IResponse response)
        {
            var contentType = response.ContentType?.KnownType;

            return (contentType == ContentType.TextCss);
        }

        public void Process(IResponse response)
        {
            response.Content = new MinifiedCss(response.Content ?? throw new InvalidOperationException("Response has not content to be minified"));
            response.ContentLength = null;
        }

    }

}
