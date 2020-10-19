using System;
using System.Reflection;
using System.Linq;
using System.IO;
using GenHTTP.Api.Content.IO;

namespace GenHTTP.Modules.IO.Providers
{

    public class ResourceDataProvider : IResource
    {
        private readonly ulong _Checksum;

        #region Get-/Setters

        public Assembly Source { get; }

        public string QualifiedName { get; }

        #endregion

        #region Initialization

        public ResourceDataProvider(Assembly source, string name)
        {
            var fqn = source.GetManifestResourceNames()
                            .FirstOrDefault(n => n.EndsWith(name));

            QualifiedName = fqn ?? throw new InvalidOperationException($"Resource '{name}' does not exist in assembly '{source}'");
            Source = source;

            using var stream = GetContent();

            _Checksum = stream.CalculateChecksum() ?? throw new InvalidOperationException("Unable to calculate checksum of assembly resource");
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
