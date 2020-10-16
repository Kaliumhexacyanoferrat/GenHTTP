﻿using System.IO;

using GenHTTP.Api.Infrastructure;
using GenHTTP.Api.Content;

namespace GenHTTP.Modules.IO.Providers
{

    public class FileDataProviderBuilder : IBuilder<IResourceProvider>
    {
        private FileInfo? _File;

        #region Functionality

        public FileDataProviderBuilder File(FileInfo file)
        {
            _File = file;
            return this;
        }

        public IResourceProvider Build()
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
