using System;
using System.Reflection;
using System.Linq;
using System.IO;
using System.Threading.Tasks;

using GenHTTP.Api.Content.IO;
using GenHTTP.Api.Protocol;

using GenHTTP.Modules.IO.Streaming;

namespace GenHTTP.Modules.IO.Embedded
{

    public sealed class EmbeddedResource : IResource
    {
        private ulong? _Checksum;

        #region Get-/Setters

        public Assembly Source { get; }

        public string QualifiedName { get; }

        public string? Name { get; }

        public DateTime? Modified { get; }

        public FlexibleContentType? ContentType { get; }

        public ulong? Length { get; }

        #endregion

        #region Initialization

        public EmbeddedResource(Assembly source, string path, string? name, FlexibleContentType? contentType, DateTime? modified)
        {
            var fqn = source.GetManifestResourceNames()
                            .FirstOrDefault(n => (n == path) || n.EndsWith($".{path}"));

            QualifiedName = fqn ?? throw new InvalidOperationException($"Resource '{path}' does not exist in assembly '{source}'");
            Source = source;

            Name = name;
            ContentType = contentType;
            Modified = modified;

            using var content = TryGetStream();

            Length = (ulong)content.Length;
        }

        #endregion

        #region Functionality

        public ValueTask<Stream> GetContentAsync() => new(TryGetStream());

        public async ValueTask<ulong> CalculateChecksumAsync()
        {
            if (_Checksum is null)
            {
                using var stream = TryGetStream();

                _Checksum = await stream.CalculateChecksumAsync() ?? throw new InvalidOperationException("Unable to calculate checksum of assembly resource");
            }

            return _Checksum.Value;
        }

        public async ValueTask WriteAsync(Stream target, uint bufferSize)
        {
            using var content = TryGetStream();

            await content.CopyPooledAsync(target, bufferSize);
        }

        private Stream TryGetStream() => Source.GetManifestResourceStream(QualifiedName) ?? throw new InvalidOperationException($"Unable to resolve resource '{QualifiedName}' in assembly '{Source}'");

        #endregion

    }

}
