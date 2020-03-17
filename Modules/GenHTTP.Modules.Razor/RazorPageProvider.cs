﻿using GenHTTP.Api.Modules;
using GenHTTP.Api.Modules.Templating;
using GenHTTP.Api.Protocol;

using GenHTTP.Modules.Core;
using GenHTTP.Modules.Core.General;
using GenHTTP.Modules.Core.Templating;

namespace GenHTTP.Modules.Razor
{

    public class RazorPageProvider<T> : ContentProviderBase where T : PageModel
    {

        #region Get-/Setters

        public IResourceProvider TemplateProvider { get; }

        public ModelProvider<T> ModelProvider { get; }

        public RazorRenderer<T> Renderer { get; }

        public override string? Title { get; }

        public override FlexibleContentType? ContentType => new FlexibleContentType(Api.Protocol.ContentType.TextHtml);

        #endregion

        #region Initialization

        public RazorPageProvider(IResourceProvider templateProvider, ModelProvider<T> modelProvider, string? title, ResponseModification? mod) : base(mod)
        {
            TemplateProvider = templateProvider;
            ModelProvider = modelProvider;
            Title = title;

            Renderer = new RazorRenderer<T>(TemplateProvider);
        }

        #endregion

        #region Functionality

        protected override IResponseBuilder HandleInternal(IRequest request)
        {
            if (request.HasType(RequestMethod.HEAD, RequestMethod.GET, RequestMethod.POST))
            {
                var model = ModelProvider(request);

                var content = Renderer.Render(model);

                var templateModel = new TemplateModel(request, model.Title ?? Title ?? "Untitled Page", content);

                return request.Respond()
                              .Content(templateModel);
            }

            return request.Respond(ResponseStatus.MethodNotAllowed);
        }

        #endregion

    }

}