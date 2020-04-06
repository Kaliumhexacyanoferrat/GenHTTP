using System;
using System.Collections.Generic;
using System.IO;
using System.Web;

using GenHTTP.Api.Content;
using GenHTTP.Api.Content.Templating;
using GenHTTP.Api.Protocol;

using GenHTTP.Modules.Core.General;

namespace GenHTTP.Modules.Core.Listing
{

    public class ListingRouter : IHandler
    {

        #region Get-/Setters

        private DirectoryInfo Info { get; }

        public IHandler Parent { get; }

        #endregion

        #region Initialization

        public ListingRouter(IHandler parent, string directory)
        {
            Parent = parent;

            Info = new DirectoryInfo(directory);

            if (!Info.Exists)
            {
                throw new DirectoryNotFoundException($"Directory with path '{directory}' does not exist");
            }
        }

        #endregion

        #region Functionality

        public IResponse? Handle(IRequest request)
        {
            /*current.Scope(this);

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
            }*/

            return null; // Todo!
        }

        public IEnumerable<ContentElement> GetContent(IRequest request)
        {
            throw new NotImplementedException();
        }

        /*public override IEnumerable<ContentElement> GetContent(IRequest request, string basePath)
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
        }*/

        #endregion

    }

}
