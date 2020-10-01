using System;
using System.Collections.Generic;

using GenHTTP.Api.Content;
using GenHTTP.Api.Content.Templating;
using GenHTTP.Api.Protocol;

using GenHTTP.Modules.Basics;

namespace GenHTTP.Modules.Scriban
{

    public class ScribanPageProvider<T> : IHandler where T : PageModel
    {

        #region Get-/Setters

        public IHandler Parent { get; }

        public IResourceProvider TemplateProvider { get; }

        public ModelProvider<T> ModelProvider { get; }

        public ScribanRenderer<T> Renderer { get; }

        public string? Title { get; }
        public string? Description { get; }

        #endregion

        #region Initialization

        public ScribanPageProvider(IHandler parent, IResourceProvider templateProvider, ModelProvider<T> modelProvider, string? title, string? description)
        {
            Parent = parent;

            TemplateProvider = templateProvider;
            ModelProvider = modelProvider;
            Title = title;
            Description = description;

            Renderer = new ScribanRenderer<T>(TemplateProvider);
        }

        #endregion

        #region Functionality

        public IResponse? Handle(IRequest request)
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
