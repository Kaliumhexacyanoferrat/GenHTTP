using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

using GenHTTP.Api.Content;
using GenHTTP.Api.Content.Pages;
using GenHTTP.Api.Protocol;
using GenHTTP.Api.Routing;

using GenHTTP.Content.Basic.Providers;

namespace GenHTTP.Content.Basic
{

    public class FileResources : IRouter
    {
        
        #region Get-/Setters

        public IRouter Parent { get; set; }

        public string Directory { get; }

        #endregion

        #region Initialization

        public FileResources(string directory)
        {
            Directory = directory;
        }
        
        #endregion

        #region Functionality

        public void HandleContext(IEditableRoutingContext current)
        {
            current.Scope(this);

            var file = Path.Combine(Directory, current.ScopedPath.Substring(1));

            if (File.Exists(file))
            {
                current.RegisterContent(new DownloadProvider(File.OpenRead(file), file.GuessContentType()));
            }
        }

        public IContentPage GetPage(IHttpRequest request, IHttpResponse response)
        {
            return Parent.GetPage(request, response);
        }

        public IContentProvider GetErrorHandler(IHttpRequest request, IHttpResponse response)
        {
            return Parent.GetErrorHandler(request, response);
        }

        #endregion

    }

}
