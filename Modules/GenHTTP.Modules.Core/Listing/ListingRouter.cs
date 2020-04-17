using System.Collections.Generic;
using System.IO;

using GenHTTP.Api.Content;
using GenHTTP.Api.Protocol;
using GenHTTP.Api.Routing;

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
            var root = new List<string>(this.GetRoot(this, false).Parts);

            foreach (var directory in Info.GetDirectories())
            {
                var path = new List<string>(root);
                path.Add(directory.Name);

                yield return new ContentElement(new WebPath(path, true), directory.Name, ContentType.TextHtml, null);
            }

            foreach (var file in Info.GetFiles())
            {
                var path = new List<string>(root);
                path.Add(file.Name);

                var guessed = file.Name.GuessContentType() ?? ContentType.ApplicationForceDownload;
                yield return new ContentElement(new WebPath(path, false), file.Name, guessed, null);
            }
        }

        #endregion

    }

}
