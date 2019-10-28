using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

using GenHTTP.Api.Modules;
using GenHTTP.Api.Modules.Templating;
using GenHTTP.Api.Protocol;

using GenHTTP.Modules.Core.General;
using GenHTTP.Modules.Core.Templating;

namespace GenHTTP.Modules.Core.Listing
{

    public class ListingProvider : ContentProviderBase
    {

        #region Get-/Setters

        public string Path { get; }

        #endregion

        #region Initialization

        public ListingProvider(string path, ResponseModification? modification) : base(modification)
        {
            Path = path;
        }

        #endregion

        #region Functionality

        protected override IResponseBuilder HandleInternal(IRequest request)
        {
            var scoped = request.Routing!.ScopedPath;
            
            var info = new DirectoryInfo(Path);

            var model = new ListingModel(request, info.GetDirectories(), info.GetFiles(), scoped != "/");

            var renderer = new ListingRenderer();

            var templateModel = new TemplateModel(request, $"Index of {request.Path}", renderer.Render(model));

            return request.Respond(ResponseStatus.OK).Content(templateModel);
        }

        #endregion

    }

}
