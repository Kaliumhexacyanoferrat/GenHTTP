using System.IO;
using GenHTTP.Api.Content.IO;

namespace GenHTTP.Modules.IO
{

    public class CachedResource : IResource
    {
        private ulong? _LastChecksum;

        #region Get-/Setters

        protected IResource Source { get; }

        public bool Changed => GetChecksum() != _LastChecksum;

        #endregion

        #region Initialization

        public CachedResource(IResource source)
        {
            Source = source;
        }

        #endregion

        #region Functionality

        public ulong GetChecksum() => Source.GetChecksum();

        public Stream GetContent()
        {
            _LastChecksum = GetChecksum();

            return Source.GetContent();
        }

        #endregion

    }

}
