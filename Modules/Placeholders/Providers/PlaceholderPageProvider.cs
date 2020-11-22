﻿using System.Collections.Generic;
using System.Threading.Tasks;

using GenHTTP.Api.Content;
using GenHTTP.Api.Content.IO;
using GenHTTP.Api.Content.Templating;
using GenHTTP.Api.Protocol;

using GenHTTP.Modules.Basics;

namespace GenHTTP.Modules.Placeholders.Providers
{

    public sealed class PlaceholderPageProvider<T> : IHandler where T : PageModel
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

        public async ValueTask<IResponse?> HandleAsync(IRequest request)
        {
            var renderer = new PlaceholderRender<T>(TemplateProvider);

            var model = ModelProvider(request, this);

            var content = await renderer.RenderAsync(model).ConfigureAwait(false);

            var templateModel = new TemplateModel(request, this, PageInfo, content);

            var page = await this.GetPageAsync(templateModel).ConfigureAwait(false);
                       
            return page.Build();
        }

        public IEnumerable<ContentElement> GetContent(IRequest request) => this.GetContent(request, PageInfo, ContentType.TextHtml);

        #endregion

    }

}
