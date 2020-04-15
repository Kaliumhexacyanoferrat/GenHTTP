using System;
using System.Collections.Generic;
using System.IO;

using GenHTTP.Api.Content;
using GenHTTP.Api.Protocol;

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
            var path = Path.Combine(Info.FullName, "." + request.Target.Remaining);

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
            // ToDo
            throw new NotImplementedException();
        }

        /*public override string? Route(string path, int currentDepth)
        {
            throw new NotImplementedException();
        }*/

        #endregion

    }

}
