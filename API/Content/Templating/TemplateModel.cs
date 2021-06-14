﻿using System.Threading.Tasks;

using GenHTTP.Api.Protocol;

namespace GenHTTP.Api.Content.Templating
{

    /// <summary>
    /// Model used by templates to render the actual HTML returned
    /// to the client.
    /// </summary>
    public sealed class TemplateModel : AbstractModel
    {

        #region Get-/Setters

        /// <summary>
        /// Additional information about the page to be rendered.
        /// </summary>
        public ContentInfo Meta { get; }

        /// <summary>
        /// The HTML content to be rendered within the template.
        /// </summary>
        public string Content { get; }

        #endregion

        #region Initialization

        /// <summary>
        /// Creates a new model instance.
        /// </summary>
        /// <param name="request">The request which caused this call</param>
        /// <param name="handler">The handler responsible to render the response</param>
        /// <param name="pageInfo">Information about the page to be rendered</param>
        /// <param name="content">The content to be rendered within the template</param>
        public TemplateModel(IRequest request, IHandler handler, ContentInfo pageInfo, string content) : base(request, handler)
        {
            Content = content;

            Meta = pageInfo;

            if (string.IsNullOrEmpty(Meta.Title))
            {
                Meta = Meta with { Title = "Untitled Page" };
            }
        }

        #endregion

        #region Functionality

        public override ValueTask<ulong> CalculateChecksumAsync()
        {
            unchecked
            {
                ulong hash = 17;

                hash = hash * 23 + (uint)Content.GetHashCode();
                hash = hash * 23 + (uint)Meta.GetHashCode();

                return new(hash);
            }
        }

        #endregion

    }

}

