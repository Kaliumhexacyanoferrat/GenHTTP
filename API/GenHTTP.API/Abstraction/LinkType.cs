using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GenHTTP.Api.Abstraction
{

    /// <summary>
    /// Describes the type of a link.
    /// </summary>
    /// <remarks>
    /// This enumeration provides the <see href="http://www.w3.org/TR/html4/types.html#h-6.12">link types
    /// of the DTD</see>.
    /// </remarks>
    public enum LinkType
    {
        /// <summary>
        /// Designates substitute versions for the document in which the link occurs.
        /// </summary>
        /// <remarks>
        /// When used together with the lang attribute, it implies a translated 
        /// version of the document. When used together with the media attribute,
        /// it implies a version designed for a different medium (or media).
        /// </remarks>
        Alternate,
        /// <summary>
        /// Refers to an external style sheet.
        /// </summary>
        /// <remarks>
        /// This is used together with the link type "Alternate"
        /// for user-selectable alternate style sheets.
        /// </remarks>
        Stylesheet,
        /// <summary>
        /// Refers to the first document in a collection of documents.
        /// </summary>
        /// <remarks>
        /// This link type tells search engines which document is
        /// considered by the author to be the starting point of the collection.
        /// </remarks>
        Start,
        /// <summary>
        /// Refers to the next document in a linear sequence of documents.
        /// </summary>
        /// <remarks>
        /// User agents may choose to preload the "next" document, to reduce the perceived load time.
        /// </remarks>
        Next,
        /// <summary>
        /// Refers to the previous document in an ordered series of documents.
        /// </summary>
        Prev,
        /// <summary>
        /// Refers to a document serving as a table of contents.
        /// </summary>
        Contents,
        /// <summary>
        /// Refers to a document providing an index for the current document.
        /// </summary>
        Index,
        /// <summary>
        /// Refers to a document providing a glossary of terms that pertain to the current document.
        /// </summary>
        Glossary,
        /// <summary>
        /// Refers to a copyright statement for the current document.
        /// </summary>
        Copyright,
        /// <summary>
        /// Refers to a document serving as a chapter in a collection of documents.
        /// </summary>
        Chapter,
        /// <summary>
        /// Refers to a document serving as a section in a collection of documents.
        /// </summary>
        Section,
        /// <summary>
        /// Refers to a document serving as a subsection in a collection of documents.
        /// </summary>
        Subsection,
        /// <summary>
        /// Refers to a document serving as an appendix in a collection of documents.
        /// </summary>
        Appendix,
        /// <summary>
        /// Refers to a document offering help (more information, links to other sources information, etc.).
        /// </summary>
        Help,
        /// <summary>
        /// Refers to a bookmark. A bookmark is a link to a key entry point within an extended document.
        /// </summary>
        /// <remarks>
        /// The title attribute may be used, for example, to label the bookmark.
        /// Note that several bookmarks may be defined in each document.
        /// </remarks>
        Bookmark,
        /// <summary>
        /// User-defined type.
        /// </summary>
        Other,
        /// <summary>
        /// Not yet defined.
        /// </summary>
        Unspecified
    }

}
