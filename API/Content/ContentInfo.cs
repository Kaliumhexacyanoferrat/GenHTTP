namespace GenHTTP.Api.Content
{

    /// <summary>
    /// Meta information regarding an element, allowing templates
    /// to render additional information such as the page title.
    /// </summary>
    public record ContentInfo 
    (

        /// <summary>
        /// The title of this element.
        /// </summary>
        string? Title,
        
        /// <summary>
        /// The description of this element.
        /// </summary>
        string? Description

    )
    {

        /// <summary>
        /// Creates a new content element.
        /// </summary>
        /// <returns>The newly created element</returns>
        public static ContentInfoBuilder Create() => new();

        /// <summary>
        /// An empty element with no additional information.
        /// </summary>
        public static ContentInfo Empty => new(null, null);

    }

}
