using GenHTTP.Api.Content;

namespace GenHTTP.Modules.Minification
{
    public static class Extensions
    {

        public static T Minification<T>(this T builder) where T : IHandlerBuilder<T>
        {
            builder.Add(Minify.Default());
            return builder;
        }

    }
}
