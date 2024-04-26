using NUglify;

using GenHTTP.Api.Protocol;

namespace GenHTTP.Modules.Minification.Plugins.Html
{

    public sealed class MinifiedHtml : TextBasedMinificationResult
    {

        public MinifiedHtml(IResponseContent original) : base(original) { }

        protected override string Transform(string input)
        {
            var minified = Uglify.Html(input);

            if (minified.HasErrors)
            {
                throw new UglifyException("Failed to minify HTML", minified.Errors);
            }

            return minified.Code;
        }

    }

}
