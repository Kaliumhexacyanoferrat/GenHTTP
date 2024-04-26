using GenHTTP.Api.Protocol;

using NUglify;

namespace GenHTTP.Modules.Minification.Plugins.JS
{

    public sealed class MinifiedJS : TextBasedMinificationResult
    {

        public MinifiedJS(IResponseContent original) : base(original) { }

        protected override string Transform(string input)
        {
            var minified = Uglify.Js(input);

            if (minified.HasErrors)
            {
                throw new UglifyException("Failed to minify JS", minified.Errors);
            }

            return minified.Code;
        }

    }

}
