using System.Collections.Generic;

using GenHTTP.Api.Modules;
using GenHTTP.Api.Modules.Templating;
using GenHTTP.Api.Protocol;

using GenHTTP.Modules.Core.General;

namespace GenHTTP.Modules.Core.Templating
{

    public class PlaceholderPageProvider<T> : ContentProviderBase where T : PageModel
    {

        #region Get-/Setters

        public IResourceProvider TemplateProvider { get; }

        public ModelProvider<T> ModelProvider { get; }

        public override string? Title { get; }

        public override FlexibleContentType? ContentType => new FlexibleContentType(Api.Protocol.ContentType.TextHtml);

        protected override HashSet<FlexibleRequestMethod>? SupportedMethods => _GET_POST;

        #endregion

        #region Initialization

        public PlaceholderPageProvider(IResourceProvider templateProvider, ModelProvider<T> modelProvider, string? title, ResponseModification? mod) : base(mod)
        {
            TemplateProvider = templateProvider;
            ModelProvider = modelProvider;
            Title = title;
        }

        #endregion

        #region Functionality

        protected override IResponseBuilder HandleInternal(IRequest request)
        {
            var renderer = new PlaceholderRender<T>(TemplateProvider);

            var model = ModelProvider(request);

            var content = renderer.Render(model);

            var templateModel = new TemplateModel(request, model.Title ?? Title ?? "Untitled Page", content);

            return request.Respond()
                          .Content(templateModel);
        }

        #endregion

    }

}
