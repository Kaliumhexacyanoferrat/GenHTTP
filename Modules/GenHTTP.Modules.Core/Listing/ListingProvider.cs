using System.Collections.Generic;
using System.IO;
using System.Web;

using GenHTTP.Api.Content;
using GenHTTP.Api.Content.Templating;
using GenHTTP.Api.Protocol;

using GenHTTP.Modules.Core.Templating;

namespace GenHTTP.Modules.Core.Listing
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

            var model = new ListingModel(request, info.GetDirectories(), info.GetFiles(), !request.Target.Ended);

            var renderer = new ListingRenderer();

            var templateModel = new TemplateModel(request, $"Index of {request.Target.Path}", renderer.Render(model));

            return this.Page(templateModel)
                       .Build();
        }

        public IEnumerable<ContentElement> GetContent(IRequest request)
        {
            throw new System.NotImplementedException();
        }

        #endregion

    }

}
