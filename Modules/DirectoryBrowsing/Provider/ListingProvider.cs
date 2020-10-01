using System;
using System.Collections.Generic;
using System.IO;

using GenHTTP.Api.Content;
using GenHTTP.Api.Content.Templating;
using GenHTTP.Api.Protocol;

using GenHTTP.Modules.Basics;

namespace GenHTTP.Modules.DirectoryBrowsing.Provider
{

    public class ListingProvider : IHandler
    {

        #region Get-/Setters

        public string Path { get; }

        public IHandler Parent { get; }

        #endregion

        #region Initialization

        public ListingProvider(IHandler parent, string path)
        {
            Parent = parent;
            Path = path;
        }

        #endregion

        #region Functionality

        public IResponse Handle(IRequest request)
        {
            var info = new DirectoryInfo(Path);

            var model = new ListingModel(request, this, info.GetDirectories(), info.GetFiles(), !request.Target.Ended);

            var renderer = new ListingRenderer();

            var templateModel = new TemplateModel(request, this, GetTitle(request), String.Empty,  renderer.Render(model));

            return this.Page(templateModel)
                       .Build();
        }

        public IEnumerable<ContentElement> GetContent(IRequest request) => this.GetContent(request, GetTitle(request), String.Empty, ContentType.TextHtml);

        private string GetTitle(IRequest request) => $"Index of {request.Target.Path}";

        #endregion

    }

}
