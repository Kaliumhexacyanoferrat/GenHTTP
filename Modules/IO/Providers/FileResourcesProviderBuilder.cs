﻿using System.Collections.Generic;
using System.IO;

using GenHTTP.Api.Content;
using GenHTTP.Api.Infrastructure;

namespace GenHTTP.Modules.Core.StaticContent
{

    public class FileResourcesProviderBuilder : IHandlerBuilder<FileResourcesProviderBuilder>
    {
        private DirectoryInfo? _Directory;

        private readonly List<IConcernBuilder> _Concerns = new List<IConcernBuilder>();

        #region Functionality

        public FileResourcesProviderBuilder Directory(DirectoryInfo directory)
        {
            _Directory = directory;
            return this;
        }

        public FileResourcesProviderBuilder Add(IConcernBuilder concern)
        {
            _Concerns.Add(concern);
            return this;
        }

        public IHandler Build(IHandler parent)
        {
            if (_Directory == null)
            {
                throw new BuilderMissingPropertyException("Directory");
            }

            if (!_Directory.Exists)
            {
                throw new DirectoryNotFoundException($"The given directory does not exist: '{_Directory.FullName}'");
            }

            return Concerns.Chain(parent, _Concerns, (p) => new FileResourcesProvider(parent, _Directory));
        }

        #endregion

    }

}
