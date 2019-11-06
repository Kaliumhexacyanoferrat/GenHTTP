using System;
using System.IO;
using System.Web;

using GenHTTP.Api.Modules;
using GenHTTP.Api.Modules.Templating;
using GenHTTP.Api.Routing;

using GenHTTP.Modules.Core.General;

namespace GenHTTP.Modules.Core.Listing
{

    public class ListingRouter : RouterBase
    {

        #region Get-/Setters

        private DirectoryInfo Info { get; }

        private ResponseModification? Modification { get; }

        #endregion

        #region Initialization

        public ListingRouter(string directory, ResponseModification? modification,
                             IRenderer<TemplateModel>? template, IContentProvider? errorHandler) : base(template, errorHandler)
        {
            Modification = modification;

            Info = new DirectoryInfo(directory);

            if (!Info.Exists)
            {
                throw new DirectoryNotFoundException($"Directory with path '{directory}' does not exist");
            }
        }

        #endregion

        #region Functionality

        public override void HandleContext(IEditableRoutingContext current)
        {
            current.Scope(this);

            var path = Path.Combine(Info.FullName, "." + HttpUtility.UrlDecode(current.ScopedPath));

            if (File.Exists(path))
            {
                var download = Download.FromFile(path);

                if (Modification != null)
                {
                    download.Modify(Modification);
                }

                current.RegisterContent(download.Build());
            }
            else if (Directory.Exists(path))
            {
                current.RegisterContent(new ListingProvider(path, Modification));
            }
        }

        public override string? Route(string path, int currentDepth)
        {
            throw new NotImplementedException();
        }

        #endregion

    }

}
