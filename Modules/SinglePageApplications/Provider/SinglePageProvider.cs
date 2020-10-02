using System;
using System.Collections.Generic;
using System.IO;

using GenHTTP.Api.Content;
using GenHTTP.Api.Protocol;

using GenHTTP.Modules.Basics;
using GenHTTP.Modules.IO;

namespace GenHTTP.Modules.SinglePageApplications.Provider
{

    public class SinglePageProvider : IHandler
    {
        private static readonly HashSet<string> INDEX_FILES = new HashSet<string>(StringComparer.InvariantCultureIgnoreCase)
        {
            "index.html", "index.htm"
        };

        #region Get-/Setters

        public IHandler Parent { get; }

        private DirectoryInfo Info { get; }

        private IHandler? Index { get; }

        #endregion

        #region Initialization

        public SinglePageProvider(IHandler parent, string directory)
        {
            Parent = parent;

            Info = new DirectoryInfo(directory);

            foreach (var file in Info.GetFiles())
            {
                if (INDEX_FILES.Contains(file.Name))
                {
                    Index = Download.FromFile(file)
                                    .Build(this);

                    break;
                }
            }
        }

        #endregion

        #region Functionality

        public IResponse? Handle(IRequest request)
        {
            if (request.Target.Ended)
            {
                return Index?.Handle(request);
            }
            else
            {
                var path = Path.Combine(Info.FullName, "." + request.Target.GetRemaining());

                if (File.Exists(path))
                {
                    return Download.FromFile(path)
                                   .Build(this)
                                   .Handle(request);
                }
            }

            return null;
        }

        public IEnumerable<ContentElement> GetContent(IRequest request)
        {
            var root = this.GetRoot(this, false);

            foreach (var file in Info.GetFiles())
            {
                var path = root.Edit(false)
                               .Append(file.Name)
                               .Build();

                var guessed = file.Name.GuessContentType() ?? ContentType.ApplicationForceDownload;

                var info = ContentInfo.Create()
                                     .Title(file.Name)
                                     .Build();

                yield return new ContentElement(path, info, guessed, null);
            }
        }

        #endregion

    }

}
