using System;
using System.Collections.Generic;
using System.Text;

using GenHTTP.Api.Protocol;

namespace GenHTTP.Api.Content.Pages
{

    public class ContentPageProvider : IContentProvider
    {

        #region Get-/Setters

        public IContentPage ServerPage { get; }

        public ResponseType ResponseType { get; }

        #endregion

        #region Initialization

        public ContentPageProvider(IContentPage page, ResponseType responseType)
        {
            ServerPage = page;
            ResponseType = responseType;
        }

        #endregion

        #region Functionality

        public void Handle(IHttpRequest request, IHttpResponse response)
        {
            response.Header.Type = ResponseType;

            using (var stream = ServerPage.GetStream())
            {
                response.Send(stream);
            }
        }

        #endregion

    }

}
