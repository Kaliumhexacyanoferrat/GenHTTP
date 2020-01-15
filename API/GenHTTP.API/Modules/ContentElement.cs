using System.Collections.Generic;

using GenHTTP.Api.Protocol;

namespace GenHTTP.Api.Modules
{

    public class ContentElement : IContentDescription
    {

        #region Get-/Setters

        public string Path { get; }

        public IEnumerable<ContentElement>? Children { get; }

        public string Title { get; }

        public long? Size { get; }

        public FlexibleContentType? ContentType { get; }

        #endregion

        #region Initialization

        public ContentElement(string path, string title, ContentType contentType, IEnumerable<ContentElement>? children)
            : this(path, title, new FlexibleContentType(contentType), children) { }

        public ContentElement(string path, string title, FlexibleContentType? contentType, IEnumerable<ContentElement>? children)
        {
            Path = path;
            Title = title;
            Children = children;

            ContentType = contentType;
        }

        #endregion

    }

}
