using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Linq;

using GenHTTP.Api.Modules;
using GenHTTP.Api.Modules.Templating;
using GenHTTP.Api.Protocol;
using GenHTTP.Api.Routing;

using GenHTTP.Modules.Core.General;

namespace GenHTTP.Modules.Core.StaticContent
{

    public class EmbeddedResourcesProvider : IRouter
    {
        private IRouter? _Parent;

        #region Get-/Setters

        public IRouter Parent
        {
            get { return _Parent ?? throw new InvalidOperationException("Parent has not been set");  }
            set { _Parent = value; }
        }

        private Dictionary<string, IContentProvider> QualifiedNames { get; }

        #endregion

        #region Initialization

        public EmbeddedResourcesProvider(Assembly assembly, string root)
        {
            QualifiedNames = assembly.GetManifestResourceNames()
                                     .Where(n => n.Contains(root))
                                     .Select(n => new
                                     {
                                         Key = n.Substring(n.IndexOf(root) + root.Length),
                                         Value = Download.FromResource(assembly, n).Build()
                                     })
                                     .ToDictionary(n => n.Key!, n => n.Value!);
        }

        #endregion

        #region Functionality

        public void HandleContext(IEditableRoutingContext current)
        {
            current.Scope(this);

            var identifier = current.ScopedPath.Replace('/', '.');

            if (QualifiedNames.ContainsKey(identifier))
            {
                current.RegisterContent(QualifiedNames[identifier]);
            }
        }

        public IRenderer<TemplateModel> GetRenderer()
        {
            return Parent.GetRenderer();
        }

        public IContentProvider GetErrorHandler(IRequest request, ResponseStatus responseType, Exception? cause)
        {
            return Parent.GetErrorHandler(request, responseType, cause);
        }

        public string? Route(string path, int currentDepth)
        {
            return Parent.Route(path, currentDepth);
        }

        #endregion

    }

}
