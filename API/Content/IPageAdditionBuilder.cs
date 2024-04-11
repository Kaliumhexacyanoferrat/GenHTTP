namespace GenHTTP.Api.Content
{

    /// <summary>
    /// Provides an unified way to add additional stylesheet or 
    /// script references to a page. 
    /// </summary>
    /// <remarks>
    /// When creating a page, you might want to include another
    /// script or stylesheet only on this site. This interface
    /// should be implemented by all page handler builders
    /// to allow this.
    /// </remarks>
    /// <typeparam name="TBuilder">The type of the builder to enable the builder pattern</typeparam>
    public interface IPageAdditionBuilder<out TBuilder>
    {

        /// <summary>
        /// Adds a script reference to be rendered on template level.
        /// </summary>
        /// <param name="path">The path to the script to be referenced (supports routing)</param>
        /// <param name="asynchronous">True, if the script should be loaded by the browser after the page has been rendered</param>
        /// <returns>The builder instance</returns>
        TBuilder AddScript(string path, bool asynchronous = false);

        /// <summary>
        /// Adds a style sheet reference to be rendered on template level.
        /// </summary>
        /// <param name="path">The path to the style sheet to be referenced (supports routing)</param>
        /// <returns>The builder instance</returns>
        TBuilder AddStyle(string path);

    }

}
