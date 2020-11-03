using System;
using System.Reflection;
using System.Linq;
using System.IO;
using GenHTTP.Api.Content.IO;
using GenHTTP.Api.Protocol;

namespace GenHTTP.Modules.IO.Embedded
{

    public class EmbeddedResource : IResource
    {
        private readonly ulong _Checksum;

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
                            .FirstOrDefault(n => n.EndsWith(path));

            QualifiedName = fqn ?? throw new InvalidOperationException($"Resource '{path}' does not exist in assembly '{source}'");
            Source = source;

            using var stream = GetContent();

            Length = (ulong)stream.Length;

            _Checksum = stream.CalculateChecksum() ?? throw new InvalidOperationException("Unable to calculate checksum of assembly resource");

            Name = name;
            ContentType = contentType;
            Modified = modified;
        }

        #endregion

        #region Functionality

        public Stream GetContent()
        {
            return Source.GetManifestResourceStream(QualifiedName);
        }

        public ulong GetChecksum() => _Checksum;

        #endregion

    }

}
