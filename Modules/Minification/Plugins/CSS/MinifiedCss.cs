using GenHTTP.Api.Protocol;

using NUglify;

namespace GenHTTP.Modules.Minification.Plugins.CSS
{

    public sealed class MinifiedCss : TextBasedMinificationResult
    {

        public MinifiedCss(IResponseContent original, MinificationErrors errorHandling) : base(original, errorHandling) { }

        protected override string Transform(string input, bool ignoreErrors)
        {
            var minified = Uglify.Css(input);

            if (minified.HasErrors && !ignoreErrors)
            {
                throw new UglifyException("Failed to minify CSS", minified.Errors);
            }

            return minified.Code;
        }

    }

}
