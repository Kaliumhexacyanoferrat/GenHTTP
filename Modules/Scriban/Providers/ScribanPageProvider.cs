﻿using System.Collections.Generic;

using GenHTTP.Api.Content;
using GenHTTP.Api.Content.Templating;
using GenHTTP.Api.Protocol;

using GenHTTP.Modules.Basics;

namespace GenHTTP.Modules.Scriban.Providers
{

    public class ScribanPageProvider<T> : IHandler where T : PageModel
    {

        #region Get-/Setters

        public IHandler Parent { get; }

        public IResourceProvider TemplateProvider { get; }

        public ModelProvider<T> ModelProvider { get; }

        public ScribanRenderer<T> Renderer { get; }

        public ContentInfo PageInfo { get; }

        #endregion

        #region Initialization

        public ScribanPageProvider(IHandler parent, IResourceProvider templateProvider, ModelProvider<T> modelProvider, ContentInfo pageInfo)
        {
            Parent = parent;

            TemplateProvider = templateProvider;
            ModelProvider = modelProvider;

            PageInfo = pageInfo;

            Renderer = new ScribanRenderer<T>(TemplateProvider);
        }

        #endregion

        #region Functionality

        public IResponse? Handle(IRequest request)
        {
            var model = ModelProvider(request, this);

            var content = Renderer.Render(model);

            var templateModel = new TemplateModel(request, this, PageInfo, content);

            return this.Page(templateModel)
                       .Build();
        }

        public IEnumerable<ContentElement> GetContent(IRequest request) => this.GetContent(request, PageInfo, ContentType.TextHtml);

        #endregion

    }

}
