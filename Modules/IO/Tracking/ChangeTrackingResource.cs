using System;
using System.IO;

using GenHTTP.Api.Content.IO;
using GenHTTP.Api.Protocol;

namespace GenHTTP.Modules.IO.Tracking
{

    public class ChangeTrackingResource : IResource
    {
        private ulong? _LastChecksum;

        #region Get-/Setters

        protected IResource Source { get; }

        /// <summary>
        /// True, if the content of the resource has changed
        /// since <see cref="GetContent()" /> has been called
        /// the last time.
        /// </summary>
        public bool Changed => GetChecksum() != _LastChecksum;

        public string? Name => Source.Name;

        public DateTime? Modified => Source.Modified;

        public FlexibleContentType? ContentType => Source.ContentType;

        public ulong? Length => Source.Length;

        #endregion

        #region Initialization

        public ChangeTrackingResource(IResource source)
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
