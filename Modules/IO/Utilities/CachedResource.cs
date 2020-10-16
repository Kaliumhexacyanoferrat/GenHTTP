using System.IO;

using GenHTTP.Api.Content;

namespace GenHTTP.Modules.IO
{

    public class CachedResource : IResourceProvider
    {
        private ulong? _LastChecksum;

        #region Get-/Setters

        protected IResourceProvider Source { get; }

        public bool Changed => GetChecksum() != _LastChecksum;

        #endregion

        #region Initialization

        public CachedResource(IResourceProvider source)
        {
            Source = source;
        }

        #endregion

        #region Functionality

        public ulong GetChecksum() => Source.GetChecksum();

        public Stream GetResource()
        {
            _LastChecksum = GetChecksum();

            return Source.GetResource();
        }

        #endregion

    }

}
