using NUglify;

using GenHTTP.Api.Protocol;

namespace GenHTTP.Modules.Minification.Plugins.Html
{

    public sealed class MinifiedHtml : TextBasedMinificationResult
    {

        public MinifiedHtml(IResponseContent original, MinificationErrors errorHandling) : base(original, errorHandling) { }

        protected override string Transform(string input, bool ignoreErrors)
        {
            var minified = Uglify.Html(input);

            if (minified.HasErrors && !ignoreErrors)
            {
                throw new UglifyException("Failed to minify HTML", minified.Errors);
            }

            return minified.Code;
        }

    }

}
