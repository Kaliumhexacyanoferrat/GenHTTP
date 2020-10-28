using System;
using System.IO;

using GenHTTP.Api.Content.IO;
using GenHTTP.Api.Protocol;

namespace GenHTTP.Modules.IO.Caching
{

    public class CachedResource : IResource
    {
        private ulong? _LastChecksum;

        #region Get-/Setters

        protected IResource Source { get; }

        public bool Changed => GetChecksum() != _LastChecksum;

        public string? Name => Source.Name;

        public DateTime? Modified => Source.Modified;

        public FlexibleContentType? ContentType => Source.ContentType;

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
