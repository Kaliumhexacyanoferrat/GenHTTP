namespace GenHTTP.Api.Content
{

    /// <summary>
    /// Meta information regarding an element, allowing templates
    /// to render additional information such as the page title.
    /// </summary>
    public class ContentInfo
    {

        #region Get-/Setters

        /// <summary>
        /// The title of this element.
        /// </summary>
        public string? Title { get; set; }

        /// <summary>
        /// The description of this element.
        /// </summary>
        public string? Description { get; set; }

        #endregion

        #region Initialization

        /// <summary>
        /// Creates a new content element.
        /// </summary>
        /// <returns>The newly created element</returns>
        public static ContentInfoBuilder Create() => new ContentInfoBuilder();

        #endregion

    }

}
