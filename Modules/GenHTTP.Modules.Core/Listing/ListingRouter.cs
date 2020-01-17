using System;
using System.Collections.Generic;
using System.IO;
using System.Web;

using GenHTTP.Api.Modules;
using GenHTTP.Api.Modules.Templating;
using GenHTTP.Api.Protocol;
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

        public override IEnumerable<ContentElement> GetContent(IRequest request, string basePath)
        {
            foreach (var directory in Info.GetDirectories())
            {
                yield return new ContentElement($"{basePath}{directory.Name}/", directory.Name, ContentType.TextHtml, null);
            }

            foreach (var file in Info.GetFiles())
            {
                var guessed = file.Name.GuessContentType() ?? ContentType.ApplicationForceDownload;
                yield return new ContentElement($"{basePath}{file.Name}", file.Name, guessed, null);
            }
        }

        public override string? Route(string path, int currentDepth)
        {
            throw new NotImplementedException();
        }
        
        #endregion

    }

}
