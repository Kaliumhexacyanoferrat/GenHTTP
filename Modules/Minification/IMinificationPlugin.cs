using GenHTTP.Api.Protocol;

using GenHTTP.Modules.Minification.Concern;

namespace GenHTTP.Modules.Minification
{

    /// <summary>
    /// Implement this interface to add a new plugin that can be added
    /// to the <see cref="MinifyBuilder" />.
    /// </summary>
    public interface IMinificationPlugin
    {

        /// <summary>
        /// Checks, whether the given response can be minified by this plugin.
        /// </summary>
        /// <param name="response">The response to be minified</param>
        /// <returns>true, if this plugin is capable of minifying the response</returns>
        bool Supports(IResponse response);

        /// <summary>
        /// Process the given response by replacing it's content with a minified version.
        /// </summary>
        /// <param name="response">The response to be minified</param>
        /// <remarks>
        /// Please ensure that your plugin properly sets the content length on the response
        /// (e.g. by setting it to null).
        /// </remarks>
        void Process(IResponse response);

    }

}
