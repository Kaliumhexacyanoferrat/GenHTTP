using GenHTTP.Api.Protocol;

using NUglify;

namespace GenHTTP.Modules.Minification.Plugins.CSS
{

    public sealed class MinifiedCss : TextBasedMinificationResult
    {

        public MinifiedCss(IResponseContent original) : base(original) { }

        protected override string Transform(string input)
        {
            var minified = Uglify.Css(input);

            if (minified.HasErrors)
            {
                throw new UglifyException("Failed to minify CSS", minified.Errors);
            }

            return minified.Code;
        }

    }

}
