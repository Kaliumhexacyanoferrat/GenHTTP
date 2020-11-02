﻿using System.Collections.Generic;

using GenHTTP.Api.Content;
using GenHTTP.Api.Content.IO;
using GenHTTP.Api.Content.Templating;
using GenHTTP.Api.Protocol;

using GenHTTP.Modules.Basics;
using GenHTTP.Modules.IO;

namespace GenHTTP.Modules.DirectoryBrowsing.Provider
{

    public class ListingProvider : IHandler
    {

        #region Get-/Setters

        public IHandler Parent { get; }

        public IResourceContainer Container { get; }

        #endregion

        #region Initialization

        public ListingProvider(IHandler parent, IResourceContainer container)
        {
            Parent = parent;
            Container = container;
        }

        #endregion

        #region Functionality

        public IResponse Handle(IRequest request)
        {
            var model = new ListingModel(request, this, Container, !request.Target.Ended);

            var renderer = new ListingRenderer();

            var templateModel = new TemplateModel(request, this, GetPageInfo(request), renderer.Render(model));

            return this.Page(templateModel)
                       .Build();
        }

        public IEnumerable<ContentElement> GetContent(IRequest request)
        {
            return Container.GetContent(request, this, (path, children) =>
            {
                var info = new ContentInfo()
                {
                    Title = $"Index of {path}"
                };

                return new ContentElement(path, info, ContentType.TextHtml, children);
            });
        }

        private ContentInfo GetPageInfo(IRequest request)
        {
            return ContentInfo.Create()
                              .Title($"Index of {request.Target.Path}")
                              .Build();
        }

        #endregion

    }

}
