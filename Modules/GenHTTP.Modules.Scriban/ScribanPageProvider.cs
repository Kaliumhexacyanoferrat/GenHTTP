using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

using GenHTTP.Api.Modules;
using GenHTTP.Api.Modules.Templating;
using GenHTTP.Api.Protocol;

using GenHTTP.Modules.Core;
using GenHTTP.Modules.Core.General;
using GenHTTP.Modules.Core.Templating;

namespace GenHTTP.Modules.Scriban
{

    public class ScribanPageProvider<T> : ContentProviderBase where T : PageModel
    {

        #region Get-/Setters

        public IResourceProvider TemplateProvider { get; }

        public ModelProvider<T> ModelProvider { get; }

        public ScribanRenderer<T> Renderer { get; }

        public string? Title { get; }

        #endregion

        #region Initialization

        public ScribanPageProvider(IResourceProvider templateProvider, ModelProvider<T> modelProvider, string? title, ResponseModification? mod) : base(mod)
        {
            TemplateProvider = templateProvider;
            ModelProvider = modelProvider;
            Title = title;

            Renderer = new ScribanRenderer<T>(TemplateProvider);
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
