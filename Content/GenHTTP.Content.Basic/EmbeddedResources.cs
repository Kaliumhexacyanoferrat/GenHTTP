using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

using GenHTTP.Api.Content;
using GenHTTP.Api.Content.Pages;
using GenHTTP.Api.Protocol;
using GenHTTP.Api.Routing;

using GenHTTP.Content.Basic.Providers;

namespace GenHTTP.Content.Basic
{

    public class EmbeddedResources : IRouter
    {

        #region Get-/Setters

        public IRouter Parent { get; set; }

        public string Root { get; }

        public Assembly Assembly { get; }

        #endregion

        #region Initialization

        public EmbeddedResources(string root) : this(Assembly.GetCallingAssembly(), root)
        {

        }

        public EmbeddedResources(Assembly assembly, string root)
        {
            Assembly = assembly;
            Root = root;
        }

        #endregion

        #region Functionality

        public void HandleContext(IEditableRoutingContext current)
        {
            current.Scope(this);

            var identifier = Root + current.ScopedPath.Replace('/', '.');

            var stream = Assembly.GetManifestResourceStream(identifier);

            if (stream != null)
            {
                current.RegisterContent(new DownloadProvider(stream, identifier.GuessContentType()));
            }
        }
        
        public IContentPage GetPage(bool error)
        {
            return Parent.GetPage(error);
        }

        public IContentProvider GetProvider(ResponseType responseType, IRoutingContext context)
        {
            return Parent.GetProvider(responseType, context);
        }
        
        #endregion

    }

}
