using System;
using System.Collections.Generic;
using System.Text;

using GenHTTP.Api.Protocol;

namespace GenHTTP.Content.Templating
{

    internal class ScribanTemplateViewModel
    {

        #region Get-/Setters

        public IHttpRequest Request { get; }

        public IHttpResponse Response { get; }

        public string Title { get; set; }

        public string Content { get; set; }

        #endregion

        #region Initialization

        public ScribanTemplateViewModel(IHttpRequest request, IHttpResponse response)
        {
            Request = request;
            Response = response;

            Title = "";
            Content = "";
        }

        #endregion

    }

}
