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

        #endregion

        #region Initialization

        public PlaceholderPageProvider(IResourceProvider templateProvider, IPageProvider<T> modelProvider)
        {
            TemplateProvider = templateProvider;
            ModelProvider = modelProvider;
        }

        #endregion

        #region Functionality

        public void Handle(IHttpRequest request, IHttpResponse response)
        {
            var renderer = new PlaceholderRender<T>(TemplateProvider);

            var model = ModelProvider.GetModel(request, response);

            var content = renderer.Render(model);

            var templateModel = new TemplateModel(request, response, model.Title ?? "Untitled Page", content);

            response.Send(templateModel, request);
        }

        #endregion

    }

}
