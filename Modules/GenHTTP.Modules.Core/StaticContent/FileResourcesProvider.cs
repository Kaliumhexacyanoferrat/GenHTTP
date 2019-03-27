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
        
        #region Get-/Setters

        public IRouter Parent { get; set; }

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

        public IContentProvider GetErrorHandler(IHttpRequest request, IHttpResponse response)
        {
            return Parent.GetErrorHandler(request, response);
        }

        #endregion

    }

}
