using System;
using System.Collections.Generic;

using GenHTTP.Api.Content;
using GenHTTP.Api.Content.Templating;
using GenHTTP.Api.Protocol;

using GenHTTP.Modules.Basics;

namespace GenHTTP.Modules.Razor
{

    public class RazorPageProvider<T> : IHandler where T : PageModel
    {

        #region Get-/Setters

        public IHandler Parent { get; }

        public IResourceProvider TemplateProvider { get; }

        public ModelProvider<T> ModelProvider { get; }

        public RazorRenderer<T> Renderer { get; }

        public string? Title { get; }
        public string? Description { get; }

        #endregion

        #region Initialization

        public RazorPageProvider(IHandler parent, IResourceProvider templateProvider, ModelProvider<T> modelProvider, string? title, string? description)
        {
            Parent = parent;

            TemplateProvider = templateProvider;
            ModelProvider = modelProvider;
            Title = title;
            Description = description;

            Renderer = new RazorRenderer<T>(TemplateProvider);
        }

        #endregion

        #region Functionality

        public IResponse Handle(IRequest request)
        {
            var model = ModelProvider(request, this);

            var content = Renderer.Render(model);

            var templateModel = new TemplateModel(request, this, model.Title ?? Title ?? "Untitled Page", model.Description ?? Description ?? String.Empty, content);

            return this.Page(templateModel)
                       .Build();
        }

        public IEnumerable<ContentElement> GetContent(IRequest request) => this.GetContent(request, Title ?? "Untitled Page", Description ?? String.Empty, ContentType.TextHtml);

        #endregion

    }

}
