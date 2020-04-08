﻿using System.Collections.Generic;

using GenHTTP.Api.Content;
using GenHTTP.Api.Content.Templating;
using GenHTTP.Api.Protocol;

using GenHTTP.Modules.Core.Templating;

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

        #endregion

        #region Initialization

        public RazorPageProvider(IHandler parent, IResourceProvider templateProvider, ModelProvider<T> modelProvider, string? title)
        {
            Parent = parent;

            TemplateProvider = templateProvider;
            ModelProvider = modelProvider;
            Title = title;

            Renderer = new RazorRenderer<T>(TemplateProvider);
        }

        #endregion

        #region Functionality

        public IResponse Handle(IRequest request)
        {
            var model = ModelProvider(request);

            var content = Renderer.Render(model);

            var templateModel = new TemplateModel(request, model.Title ?? Title ?? "Untitled Page", content);

            return request.Respond()
                          .Content(templateModel)
                          .Build();
        }

        public IEnumerable<ContentElement> GetContent(IRequest request)
        {
            throw new System.NotImplementedException();
        }

        #endregion

    }

}
