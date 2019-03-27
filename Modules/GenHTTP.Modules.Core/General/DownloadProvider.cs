using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

using GenHTTP.Api.Modules;
using GenHTTP.Api.Protocol;

namespace GenHTTP.Modules.Core.General
{

    public class DownloadProvider : IContentProvider
    {

        #region Get-/Setters

        public IResourceProvider ResourceProvider { get; }

        public ContentType ContentType { get; }

        #endregion

        #region Initialization

        public DownloadProvider(IResourceProvider resourceProvider, ContentType contentType)
        {
            ResourceProvider = resourceProvider;
            ContentType = contentType;
        }

        #endregion

        #region Functionality

        public void Handle(IHttpRequest request, IHttpResponse response)
        {
            response.Header.ContentType = ContentType;

            using (var stream = ResourceProvider.GetResource())
            {
                response.Send(stream);
            }
        }

        #endregion

    }

}
