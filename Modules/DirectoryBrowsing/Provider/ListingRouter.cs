using System.Collections.Generic;
using System.IO;

using GenHTTP.Api.Content;
using GenHTTP.Api.Protocol;

using GenHTTP.Modules.Core;
using GenHTTP.Modules.IO;

namespace GenHTTP.Modules.DirectoryBrowsing.Provider
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
            var path = Path.Combine(Info.FullName, "." + request.Target.GetRemaining());

            if (File.Exists(path))
            {
                return Download.FromFile(path)
                               .Build(this)
                               .Handle(request);
            }
            else if (Directory.Exists(path))
            {
                return new ListingProvider(this, path).Handle(request);
            }

            return null;
        }

        public IEnumerable<ContentElement> GetContent(IRequest request)
        {
            var root = this.GetRoot(this, false);

            foreach (var directory in Info.GetDirectories())
            {
                var path = root.Edit(true)
                               .Append(directory.Name)
                               .Build();

                yield return new ContentElement(path, directory.Name, ContentType.TextHtml, null);
            }

            foreach (var file in Info.GetFiles())
            {
                var path = root.Edit(false)
                               .Append(file.Name)
                               .Build();

                var guessed = file.Name.GuessContentType() ?? ContentType.ApplicationForceDownload;

                yield return new ContentElement(path, file.Name, guessed, null);
            }
        }

        #endregion

    }

}
