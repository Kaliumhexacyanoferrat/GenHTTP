using System;
using System.IO;
using System.Threading.Tasks;
using GenHTTP.Api.Content.IO;
using GenHTTP.Api.Protocol;

namespace GenHTTP.Modules.IO.Tracking
{

    public sealed class ChangeTrackingResource : IResource
    {
        private ulong? _LastChecksum;

        #region Get-/Setters

        private IResource Source { get; }

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

        public async ValueTask<Stream> GetContentAsync()
        {
            _LastChecksum = await CalculateChecksumAsync();

            return await Source.GetContentAsync();
        }

        public ValueTask<ulong> CalculateChecksumAsync()
        {
            return Source.CalculateChecksumAsync();
        }

        /// <summary>
        /// True, if the content of the resource has changed
        /// since <see cref="GetContentAsync()" /> has been called
        /// the last time.
        /// </summary>
        public async ValueTask<bool> HasChanged()
        {
            return await CalculateChecksumAsync() != _LastChecksum;
        }

        #endregion

    }

}
