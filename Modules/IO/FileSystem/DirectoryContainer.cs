using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

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

        public IAsyncEnumerable<IResourceNode> GetNodes() => GetNodesInternal().ToAsyncEnumerable();

        private IEnumerable<IResourceNode> GetNodesInternal()
        {
            foreach (var directory in Directory.EnumerateDirectories())
            {
                yield return new DirectoryNode(directory, this);
            }
        }

        public IAsyncEnumerable<IResource> GetResources() => GetResourcesInternal().ToAsyncEnumerable();

        public IEnumerable<IResource> GetResourcesInternal()
        {
            foreach (var file in Directory.EnumerateFiles())
            {
                yield return Resource.FromFile(file).Build();
            }
        }

        public ValueTask<IResourceNode?> TryGetNodeAsync(string name)
        {
            var path = Path.Combine(Directory.FullName, name);

            var directory = new DirectoryInfo(path);

            if (directory.Exists)
            {
                return new(new DirectoryNode(directory, this));
            }

            return new();
        }

        public ValueTask<IResource?> TryGetResourceAsync(string name)
        {
            var path = Path.Combine(Directory.FullName, name);

            var file = new FileInfo(path);

            if (file.Exists)
            {
                return new(Resource.FromFile(file).Build());
            }

            return new();
        }

        #endregion

    }

}
