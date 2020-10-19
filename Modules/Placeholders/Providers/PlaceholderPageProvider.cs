using System.Collections.Generic;

using GenHTTP.Api.Content;
using GenHTTP.Api.Content.IO;
using GenHTTP.Api.Content.Templating;
using GenHTTP.Api.Protocol;

using GenHTTP.Modules.Basics;

namespace GenHTTP.Modules.Placeholders.Providers
{

    public class PlaceholderPageProvider<T> : IHandler where T : PageModel
    {

        #region Get-/Setters

        public IHandler Parent { get; }

        public IResource TemplateProvider { get; }

        public ModelProvider<T> ModelProvider { get; }

        public ContentInfo PageInfo { get; }

        #endregion

        #region Initialization

        public PlaceholderPageProvider(IHandler parent, IResource templateProvider, ModelProvider<T> modelProvider, ContentInfo pageInfo)
        {
            Parent = parent;

            TemplateProvider = templateProvider;
            ModelProvider = modelProvider;

            PageInfo = pageInfo;
        }

        #endregion

        #region Functionality

        public IResponse Handle(IRequest request)
        {
            var renderer = new PlaceholderRender<T>(TemplateProvider);

            var model = ModelProvider(request, this);

            var content = renderer.Render(model);

            var templateModel = new TemplateModel(request, this, PageInfo, content);

            return this.Page(templateModel)
                       .Build();
        }

        public IEnumerable<ContentElement> GetContent(IRequest request) => this.GetContent(request, PageInfo, ContentType.TextHtml);

        #endregion

    }

}
