using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

using GenHTTP.Api.Modules;

namespace GenHTTP.Modules.Core.Resource
{

    public class FileDataProvider : IResourceProvider
    {

        #region Get-/Setters

        public FileInfo File { get; }

        #endregion

        #region Initialization

        public FileDataProvider(FileInfo file)
        {
            if (!file.Exists)
            {
                throw new FileNotFoundException("Template file does not exist", file.FullName);
            }

            File = file;
        }

        #endregion

        #region Functionality

        public Stream GetResource()
        {
            return File.OpenRead();
        }
        
        #endregion

    }

}
