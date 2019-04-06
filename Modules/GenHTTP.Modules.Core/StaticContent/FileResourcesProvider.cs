using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

using GenHTTP.Api.Modules;
using GenHTTP.Api.Modules.Templating;
using GenHTTP.Api.Protocol;
using GenHTTP.Api.Routing;

namespace GenHTTP.Modules.Core.StaticContent
{

    public class FileResourcesProvider : IRouter
    {
        private IRouter? _Parent;

        #region Get-/Setters

        public IRouter Parent
        {
            get { return _Parent ?? throw new InvalidOperationException("Parent has not been set"); }
            set { _Parent = value; }
        }

        public DirectoryInfo Directory { get; }

        #endregion

        #region Initialization

        public FileResourcesProvider(DirectoryInfo directory)
        {
            Directory = directory;
        }

        #endregion

        #region Functionality

        public void HandleContext(IEditableRoutingContext current)
        {
            current.Scope(this);

            var file = Path.Combine(Directory.FullName, current.ScopedPath.Substring(1));

            if (File.Exists(file))
            {
                current.RegisterContent(Download.FromFile(file).Build());
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
