using GenHTTP.Api.Protocol;
using GenHTTP.Api.Routing;
using System.Collections.Generic;

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
        public string Path { get; }

        /// <summary>
        /// The children available below this element, if any.
        /// </summary>
        public IEnumerable<ContentElement>? Children { get; }

        /// <summary>
        /// The title of this element.
        /// </summary>
        public string Title { get; }

        /// <summary>
        /// Meta description
        /// </summary>
        public string Description { get; }

        /// <summary>
        /// The content type of the element. Can be used to filter
        /// the elements available.
        /// </summary>
        public FlexibleContentType ContentType { get; }

        #endregion

        #region Initialization

        public ContentElement(WebPath path, string title, string description, ContentType contentType,
            IEnumerable<ContentElement>? children = null)
            : this(path.ToString(), title, description, new FlexibleContentType(contentType), children) { }

        public ContentElement(string path, string title, string description, ContentType contentType, IEnumerable<ContentElement>? children = null)
            : this(path, title, description, new FlexibleContentType(contentType), children) { }

        public ContentElement(WebPath path, string title, string description, FlexibleContentType contentType, IEnumerable<ContentElement>? children = null)
             : this(path.ToString(), title, description, contentType, children) { }

        public ContentElement(string path, string title, string description, FlexibleContentType contentType, IEnumerable<ContentElement>? children = null)
        {
            Path = path;
            Title = title;
            Description = description;
            Children = children;

            ContentType = contentType;
        }

        #endregion

    }

}
