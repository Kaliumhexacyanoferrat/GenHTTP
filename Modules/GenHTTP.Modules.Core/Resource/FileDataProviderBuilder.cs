using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

using GenHTTP.Api.Infrastructure;

namespace GenHTTP.Modules.Core.Resource
{

    public class FileDataProviderBuilder : IBuilder<FileDataProvider>
    {
        private FileInfo? _File;

        #region Functionality

        public FileDataProviderBuilder File(string path)
        {
            _File = new FileInfo(path);
            return this;
        }

        public FileDataProviderBuilder File(FileInfo file)
        {
            _File = file;
            return this;
        }

        public FileDataProvider Build()
        {
            if (_File == null)
            {
                throw new BuilderMissingPropertyException("File");
            }

            if (!_File.Exists)
            {
                throw new FileNotFoundException("The given file does not exist", _File.FullName);
            }

            return new FileDataProvider(_File);
        }

        #endregion

    }

}
