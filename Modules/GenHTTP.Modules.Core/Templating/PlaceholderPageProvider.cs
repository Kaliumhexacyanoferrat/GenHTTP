using System.Collections.Generic;

using GenHTTP.Api.Content;
using GenHTTP.Api.Content.Templating;
using GenHTTP.Api.Protocol;

namespace GenHTTP.Modules.Core.Templating
{

    public class PlaceholderPageProvider<T> : IHandler where T : PageModel
    {

        #region Get-/Setters

        public IHandler Parent { get; }

        public IResourceProvider TemplateProvider { get; }

        public ModelProvider<T> ModelProvider { get; }

        public string? Title { get; }

        #endregion

        #region Initialization

        public PlaceholderPageProvider(IHandler parent, IResourceProvider templateProvider, ModelProvider<T> modelProvider, string? title)
        {
            Parent = parent;

            TemplateProvider = templateProvider;
            ModelProvider = modelProvider;
            Title = title;
        }

        #endregion

        #region Functionality

        public IResponse Handle(IRequest request)
        {
            var renderer = new PlaceholderRender<T>(TemplateProvider);

            var model = ModelProvider(request, this);

            var content = renderer.Render(model);

            var templateModel = new TemplateModel(request, this, model.Title ?? Title ?? "Untitled Page", content);

            return this.Page(templateModel)
                       .Build();
        }

        public IEnumerable<ContentElement> GetContent(IRequest request) => this.GetContent(request, Title ?? "Untitled Page", ContentType.TextHtml);

        #endregion

    }

}
