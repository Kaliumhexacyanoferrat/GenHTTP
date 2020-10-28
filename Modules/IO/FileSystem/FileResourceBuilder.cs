using System;
using System.IO;

using GenHTTP.Api.Content.IO;
using GenHTTP.Api.Infrastructure;
using GenHTTP.Api.Protocol;

namespace GenHTTP.Modules.IO.FileSystem
{

    public class FileResourceBuilder : IBuilder<IResource>, IResourceMetaDataBuilder<FileResourceBuilder>
    {
        private FileInfo? _File;

        private string? _Name;

        private FlexibleContentType? _Type;

        private DateTime? _Modified;

        #region Functionality

        public FileResourceBuilder File(FileInfo file)
        {
            _File = file;
            return this;
        }

        public FileResourceBuilder Name(string name)
        {
            _Name = name;
            return this;
        }

        public FileResourceBuilder Type(FlexibleContentType contentType)
        {
            _Type = contentType;
            return this;
        }

        public FileResourceBuilder Modified(DateTime modified)
        {
            _Modified = modified;
            return this;
        }

        public IResource Build()
        {
            var file = _File ?? throw new BuilderMissingPropertyException("file");

            if (!file.Exists)
            {
                throw new FileNotFoundException("The given file does not exist", file.FullName);
            }

            return new FileResource(file, _Name, _Type, _Modified);
        }

        #endregion

    }

}
