using GenHTTP.Api.Protocol;

using NUglify;

namespace GenHTTP.Modules.Minification.Plugins.JS
{

    public sealed class MinifiedJS : TextBasedMinificationResult
    {

        public MinifiedJS(IResponseContent original, MinificationErrors errorHandling) : base(original, errorHandling) { }

        protected override string Transform(string input, bool ignoreErrors)
        {
            var minified = Uglify.Js(input);

            if (minified.HasErrors && !ignoreErrors)
            {
                throw new UglifyException("Failed to minify JS", minified.Errors);
            }

            return minified.Code;
        }

    }

}
