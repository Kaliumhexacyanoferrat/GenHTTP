using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

using GenHTTP.Api.Modules;
using GenHTTP.Api.Modules.Templating;
using GenHTTP.Api.Protocol;

namespace GenHTTP.Modules.Core.Templating
{

    public class PlaceholderPageProvider<T> : IContentProvider where T : PageModel
    {

        #region Get-/Setters

        public IResourceProvider TemplateProvider { get; }

        public IPageProvider<T> ModelProvider { get; }

        public string? Title { get; }

        #endregion

        #region Initialization

        public PlaceholderPageProvider(IResourceProvider templateProvider, IPageProvider<T> modelProvider, string? title)
        {
            TemplateProvider = templateProvider;
            ModelProvider = modelProvider;
        }

        #endregion

        #region Functionality

        public IResponseBuilder Handle(IRequest request)
        {
            if (request.HasType(RequestType.HEAD, RequestType.GET, RequestType.POST))
            {
                var renderer = new PlaceholderRender<T>(TemplateProvider);

                var model = ModelProvider.GetModel(request);

                var content = renderer.Render(model);

                var templateModel = new TemplateModel(request, model.Title ?? Title ?? "Untitled Page", content);

                return request.Respond()
                              .Content(templateModel);
            }

            return request.Respond(ResponseType.MethodNotAllowed);
        }

        #endregion

    }

}
