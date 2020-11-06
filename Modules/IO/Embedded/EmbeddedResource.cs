using System;
using System.Reflection;
using System.Linq;
using System.IO;
using System.Threading.Tasks;

using GenHTTP.Api.Content.IO;
using GenHTTP.Api.Protocol;

namespace GenHTTP.Modules.IO.Embedded
{

    public class EmbeddedResource : IResource
    {
        private ulong? _Checksum, _Length;

        #region Get-/Setters

        public Assembly Source { get; }

        public string QualifiedName { get; }

        public string? Name { get; }

        public DateTime? Modified { get; }

        public FlexibleContentType? ContentType { get; }

        public ulong? Length => _Length;

        #endregion

        #region Initialization

        public EmbeddedResource(Assembly source, string path, string? name, FlexibleContentType? contentType, DateTime? modified)
        {
            var fqn = source.GetManifestResourceNames()
                            .FirstOrDefault(n => n.EndsWith(path));

            QualifiedName = fqn ?? throw new InvalidOperationException($"Resource '{path}' does not exist in assembly '{source}'");
            Source = source;

            Name = name;
            ContentType = contentType;
            Modified = modified;
        }

        #endregion

        #region Functionality

        public ValueTask<Stream> GetContentAsync()
        {
            return new ValueTask<Stream>(Source.GetManifestResourceStream(QualifiedName));
        }

        public async ValueTask<ulong> CalculateChecksumAsync()
        {
            if (_Checksum == null)
            {
                using var stream = await GetContentAsync();

                _Checksum = await stream.CalculateChecksumAsync() ?? throw new InvalidOperationException("Unable to calculate checksum of assembly resource");
                _Length = (ulong)stream.Length;
            }

            return _Checksum.Value;
        }

        #endregion

    }

}
