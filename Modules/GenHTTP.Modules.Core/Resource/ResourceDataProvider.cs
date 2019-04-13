using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Linq;
using System.IO;

using GenHTTP.Api.Modules;

namespace GenHTTP.Modules.Core.Resource
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
                            .Where(n => n.EndsWith(name))
                            .FirstOrDefault();

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
