using System.Collections.Generic;
using GenHTTP.Api.Protocol;
using GenHTTP.Api.Routing;

namespace GenHTTP.Api.Content
{

    /// <summary>
    /// Describes an element that is provided by this server instance.
    /// </summary>
    public sealed class ContentElement
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
        /// Additional meta information describing this element.
        /// </summary>
        public ContentInfo Details { get; }

        /// <summary>
        /// The content type of the element. Can be used to filter
        /// the elements available.
        /// </summary>
        public FlexibleContentType ContentType { get; }

        #endregion

        #region Initialization

        public ContentElement(WebPath path, ContentInfo details, ContentType contentType,IEnumerable<ContentElement>? children = null)
            : this(path.ToString(), details, new FlexibleContentType(contentType), children) { }

        public ContentElement(string path, ContentInfo details, ContentType contentType, IEnumerable<ContentElement>? children = null)
            : this(path, details, new FlexibleContentType(contentType), children) { }

        public ContentElement(WebPath path, ContentInfo details, FlexibleContentType contentType, IEnumerable<ContentElement>? children = null)
             : this(path.ToString(), details, contentType, children) { }

        public ContentElement(string path, ContentInfo details, FlexibleContentType contentType, IEnumerable<ContentElement>? children = null)
        {
            Path = path;
            Details = details;

            Children = children;

            ContentType = contentType;
        }

        #endregion

    }

}
