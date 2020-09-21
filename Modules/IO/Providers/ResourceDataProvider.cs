using System;
using System.Reflection;
using System.Linq;
using System.IO;

using GenHTTP.Api.Content;

namespace GenHTTP.Modules.IO.Providers
{

    public class ResourceDataProvider : IResourceProvider
    {

        #region Get-/Setters

        public Assembly Source { get; }

        public string QualifiedName { get; }

        public bool AllowCache => true;

        #endregion

        #region Initialization

        public ResourceDataProvider(Assembly source, string name)
        {
            var fqn = source.GetManifestResourceNames()
                            .FirstOrDefault(n => n.EndsWith(name));

            QualifiedName = fqn ?? throw new InvalidOperationException($"Resource '{name}' does not exist in assembly '{source}'");
            Source = source;
        }

        #endregion

        #region Functionality

        public Stream GetResource()
        {
            return Source.GetManifestResourceStream(QualifiedName);
        }

        #endregion

    }

}
