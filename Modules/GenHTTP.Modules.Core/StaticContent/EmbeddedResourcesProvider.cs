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

        #region Get-/Setters

        public IRouter Parent { get; set; }

        protected Dictionary<string, IContentProvider> QualifiedNames { get; }

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
                                     .ToDictionary(n => n.Key!!, n => n.Value!!);
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

        public IContentProvider GetErrorHandler(IHttpRequest request, IHttpResponse response)
        {
            return Parent.GetErrorHandler(request, response);
        }
        
        #endregion

    }

}
