using System;
using System.Collections.Generic;
using System.IO;

using GenHTTP.Api.Content;
using GenHTTP.Api.Protocol;

using GenHTTP.Modules.Basics;

namespace GenHTTP.Modules.IO.Providers
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
            var scoped = request.Target.GetRemaining().ToString().Substring(1);

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
            var root = this.GetRoot(request.Server.Handler, false);

            foreach (var file in Directory.EnumerateFiles("*.*", SearchOption.AllDirectories))
            {
                var childPath = Path.GetRelativePath(Directory.FullName, file.FullName);

                var child = root.Edit(false)
                                .Append(childPath)
                                .Build();

                var info = ContentInfo.Create()
                                      .Title(file.Name)
                                      .Build();

                yield return new ContentElement(child, info, file.Name.GuessContentType() ?? ContentType.ApplicationForceDownload, null);
            }
        }

        #endregion

    }

}
