using System.Collections.Generic;
using System.IO;

using GenHTTP.Api.Content;
using GenHTTP.Api.Content.Templating;
using GenHTTP.Api.Protocol;

namespace GenHTTP.Modules.DirectoryBrowsing.Provider
{

    public class ListingModel : IBaseModel
    {

        #region Get-/Setters

        public IRequest Request { get; }

        public IHandler Handler { get; }

        public List<DirectoryInfo> Directories { get; }

        public List<FileInfo> Files { get; }

        public bool HasParent { get; }

        #endregion

        #region Intialization

        public ListingModel(IRequest request, IHandler handler, DirectoryInfo[] directories, FileInfo[] files, bool hasParent)
        {
            Request = request;
            Handler = handler;

            Directories = new List<DirectoryInfo>(directories);
            Files = new List<FileInfo>(files);

            HasParent = hasParent;
        }

        #endregion

    }

}
