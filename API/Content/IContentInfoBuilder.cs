namespace GenHTTP.Api.Content
{

    /// <summary>
    /// Builders implementing this interface allow to
    /// customize the meta data of a content element 
    /// such as a web page.
    /// </summary>
    /// <typeparam name="T">The type of the builder implementing this interface</typeparam>
    public interface IContentInfoBuilder<out T>
    {

        /// <summary>
        /// Sets the title of the content element, e.g.
        /// for usage in a site map or as the title
        /// of a web page.
        /// </summary>
        /// <param name="title">The title to be set</param>
        T Title(string title);

        /// <summary>
        /// Sets the description of the content element.
        /// </summary>
        /// <param name="description">The description to be set</param>
        T Description(string description);

    }

}
