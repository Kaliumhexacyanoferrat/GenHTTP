using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

using GenHTTP.Api.Content;
using GenHTTP.Api.Protocol;

namespace GenHTTP.Content.Basic.Providers
{

    public class DownloadProvider : IContentProvider
    {

        #region Get-/Setters

        public Stream Stream { get; }

        public ContentType ContentType { get; }

        #endregion

        #region Initialization

        public DownloadProvider(Stream stream, ContentType contentType = ContentType.ApplicationForceDownload)
        {
            Stream = stream;
            ContentType = contentType;
        }

        #endregion

        #region Functionality

        public void Handle(IHttpRequest request, IHttpResponse response)
        {
            response.Header.ContentType = ContentType;

            try
            {
                response.Send(Stream);
            }
            finally
            {
                Stream.Dispose();
            }
        }

        #endregion

    }

}
