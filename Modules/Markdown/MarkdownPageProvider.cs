using System;
using System.Collections.Generic;

using GenHTTP.Api.Content;
using GenHTTP.Api.Content.IO;
using GenHTTP.Api.Content.Templating;
using GenHTTP.Api.Protocol;

using GenHTTP.Modules.Basics;

namespace GenHTTP.Modules.Markdown
{

    public class MarkdownPageProvider<T> : IHandler where T : PageModel
    {

        #region Get-/Setters

        public IHandler Parent { get; }

        public MarkdownRenderer<T> Renderer { get; }

        public ContentInfo PageInfo { get; }

        #endregion

        #region Initialization

        public MarkdownPageProvider(IHandler parent, IResource fileProvider, ContentInfo pageInfo)
        {
            Parent = parent;

            PageInfo = pageInfo;
            Renderer = new MarkdownRenderer<T>(fileProvider);
        }

        #endregion

        #region Functionality

        public IEnumerable<ContentElement> GetContent(IRequest request) => this.GetContent(request, PageInfo, ContentType.TextHtml);

        public IResponse Handle(IRequest request)
        {
            var content = Renderer.Render(null);

            var templateModel = new TemplateModel(request, this, PageInfo, content);

            return this.Page(templateModel)
                       .Build();
        }

        #endregion

    }

}