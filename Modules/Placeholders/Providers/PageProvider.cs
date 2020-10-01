using System;
using System.Collections.Generic;

using GenHTTP.Api.Content;
using GenHTTP.Api.Content.Templating;
using GenHTTP.Api.Protocol;

using GenHTTP.Modules.Basics;

namespace GenHTTP.Modules.Placeholders.Providers
{

    public class PageProvider : IHandler
    {

        #region Get-/Setters

        public string? Title { get; }

        public string? Description { get; }

        public IResourceProvider Content { get; }

        public IHandler Parent { get; }

        #endregion

        #region Initialization

        public PageProvider(IHandler parent, string? title, string? description, IResourceProvider content)
        {
            Parent = parent;

            Title = title;
            Description = description;
            Content = content;
        }

        #endregion

        #region Functionality

        public IResponse? Handle(IRequest request)
        {
            var templateModel = new TemplateModel(request, this, Title ?? "Untitled Page", Description, Content.GetResourceAsString());

            return this.Page(templateModel)
                       .Build();
        }

        public IEnumerable<ContentElement> GetContent(IRequest request) => this.GetContent(request, Title ?? "Untitled Page", Description, ContentType.TextHtml);

        #endregion

    }

}
