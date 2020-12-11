using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;

using GenHTTP.Api.Content.IO;

namespace GenHTTP.Modules.IO.FileSystem
{

    internal class DirectoryContainer : IResourceContainer
    {

        #region Get-/Setters

        protected DirectoryInfo Directory { get; }

        public DateTime? Modified => Directory.LastWriteTimeUtc;

        #endregion

        #region Initialization

        protected DirectoryContainer(DirectoryInfo directory)
        {
            Directory = directory;
        }

        #endregion

        #region Functionality

        public IEnumerable<IResourceNode> GetNodes()
        {
            foreach (var directory in Directory.EnumerateDirectories())
            {
                yield return new DirectoryNode(directory, this);
            }
        }

        public IEnumerable<IResource> GetResources()
        {
            foreach (var file in Directory.EnumerateFiles())
            {
                yield return Resource.FromFile(file)
                                     .Build();
            }
        }

        public bool TryGetNode(string name, [MaybeNullWhen(returnValue: false)] out IResourceNode node)
        {
            var path = Path.Combine(Directory.FullName, name);

            var directory = new DirectoryInfo(path);

            if (directory.Exists)
            {
                node = new DirectoryNode(directory, this);
                return true;
            }

            node = default;
            return false;
        }

        public bool TryGetResource(string name, [MaybeNullWhen(returnValue: false)] out IResource node)
        {
            var path = Path.Combine(Directory.FullName, name);

            var file = new FileInfo(path);

            if (file.Exists)
            {
                node = Resource.FromFile(file)
                               .Build();

                return true;
            }

            node = default;
            return false;
        }

        #endregion

    }

}
