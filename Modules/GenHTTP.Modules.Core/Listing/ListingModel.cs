using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

using GenHTTP.Api.Modules.Templating;
using GenHTTP.Api.Protocol;

namespace GenHTTP.Modules.Core.Listing
{

    public class ListingModel : IBaseModel
    {

        #region Get-/Setters

        public IRequest Request { get; }

        public List<DirectoryInfo> Directories { get; }

        public List<FileInfo> Files { get; }

        public bool HasParent { get; }

        #endregion

        #region Intialization

        public ListingModel(IRequest request, DirectoryInfo[] directories, FileInfo[] files, bool hasParent)
        {
            Request = request;

            Directories = new List<DirectoryInfo>(directories);
            Files = new List<FileInfo>(files);

            HasParent = hasParent;
        }

        #endregion

    }

}
