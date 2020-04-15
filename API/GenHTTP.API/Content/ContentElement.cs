using System.Collections.Generic;

using GenHTTP.Api.Protocol;
using GenHTTP.Api.Routing;

namespace GenHTTP.Api.Content
{

    public class ContentElement
    {

        #region Get-/Setters

        public WebPath Path { get; }

        public IEnumerable<ContentElement>? Children { get; }

        public string Title { get; }

        public long? Size { get; }

        public FlexibleContentType? ContentType { get; }

        #endregion

        #region Initialization

        public ContentElement(WebPath path, string title, ContentType contentType, IEnumerable<ContentElement>? children = null)
            : this(path, title, new FlexibleContentType(contentType), children) { }

        public ContentElement(WebPath path, string title, FlexibleContentType? contentType, IEnumerable<ContentElement>? children = null)
        {
            Path = path;
            Title = title;
            Children = children;

            ContentType = contentType;
        }

        #endregion

    }

}
