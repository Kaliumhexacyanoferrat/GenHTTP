using System;

using GenHTTP.Api.Protocol;

namespace GenHTTP.Modules.Minification.Plugins.JS
{

    public sealed class JSPlugin : IMinificationPlugin
    {

        public bool Supports(IResponse response)
        {
            var contentType = response.ContentType?.KnownType;

            return (contentType == ContentType.ApplicationJavaScript) || (contentType == ContentType.TextJavaScript);
        }

        public void Process(IResponse response, MinificationErrors errorHandling)
        {
            response.Content = new MinifiedJS(response.Content ?? throw new InvalidOperationException("Response has not content to be minified"), errorHandling);
            response.ContentLength = null;
        }

    }

}
