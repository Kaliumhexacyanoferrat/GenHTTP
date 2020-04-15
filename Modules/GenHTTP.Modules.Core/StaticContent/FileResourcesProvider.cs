using System.Collections.Generic;
using System.IO;

using GenHTTP.Api.Content;
using GenHTTP.Api.Protocol;
using GenHTTP.Api.Routing;

namespace GenHTTP.Modules.Core.StaticContent
{

    public class FileResourcesProvider : IHandler
    {

        #region Get-/Setters

        public DirectoryInfo Directory { get; }

        public IHandler Parent { get; }

        #endregion

        #region Initialization

        public FileResourcesProvider(IHandler parent, DirectoryInfo directory)
        {
            Parent = parent;
            Directory = directory;
        }

        #endregion

        #region Functionality

        public IResponse? Handle(IRequest request)
        {
            var scoped = request.Target.Remaining.ToString().Substring(1);

            var file = Path.Combine(Directory.FullName, scoped);

            if (File.Exists(file))
            {
                return Download.FromFile(file)
                               .Build(this)
                               .Handle(request);
            }

            return null;
        }

        public IEnumerable<ContentElement> GetContent(IRequest request)
        {
            var root = new List<string>(this.GetRoot(request.Server.Handler, false).Parts);

            foreach (var file in Directory.EnumerateFiles("*.*", SearchOption.AllDirectories))
            {
                var childPath = Path.GetRelativePath(Directory.FullName, file.FullName);

                var child = new List<string>(root);
                child.Add(childPath);

                yield return new ContentElement(new WebPath(child, false), file.Name, file.Name.GuessContentType() ?? ContentType.ApplicationForceDownload, null);
            }
        }

        #endregion

    }

}
