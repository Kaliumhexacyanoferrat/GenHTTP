
namespace GenHTTP.Modules.Minification
{

    /// <summary>
    /// Defines the error handling strategy to be respected by a plugin.
    /// </summary>
    public enum MinificationErrors
    {

        /// <summary>
        /// Ignores errors raised by the plugin and serves the generated
        /// content anyway.
        /// </summary>
        /// <remarks>
        /// For plugins where this is not possible this should behave like
        /// <see cref="ServeOriginal" />.
        /// </remarks>
        Ignore,

        /// <summary>
        /// Serves the original, not minified content in case of an error.
        /// </summary>
        ServeOriginal,

        /// <summary>
        /// Throws an exception, resulting in a HTTP 500 error message being
        /// rendered.
        /// </summary>
        Throw

    }

}
