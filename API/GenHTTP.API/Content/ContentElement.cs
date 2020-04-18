using System.Collections.Generic;

using GenHTTP.Api.Protocol;
using GenHTTP.Api.Routing;

namespace GenHTTP.Api.Content
{

    /// <summary>
    /// Describes an element that is provided by this server instance.
    /// </summary>
    public class ContentElement
    {

        #region Get-/Setters

        /// <summary>
        /// The absolute path of the element.
        /// </summary>
        public WebPath Path { get; }

        /// <summary>
        /// The children available below this element, if any.
        /// </summary>
        public IEnumerable<ContentElement>? Children { get; }

        /// <summary>
        /// The title of this element.
        /// </summary>
        public string Title { get; }

        /// <summary>
        /// The content type of the element. Can be used to filter
        /// the elements available.
        /// </summary>
        public FlexibleContentType ContentType { get; }

        #endregion

        #region Initialization

        public ContentElement(WebPath path, string title, ContentType contentType, IEnumerable<ContentElement>? children = null)
            : this(path, title, new FlexibleContentType(contentType), children) { }

        public ContentElement(WebPath path, string title, FlexibleContentType contentType, IEnumerable<ContentElement>? children = null)
        {
            Path = path;
            Title = title;
            Children = children;

            ContentType = contentType;
        }

        #endregion

    }

}
