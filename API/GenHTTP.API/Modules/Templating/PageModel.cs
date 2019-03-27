using System;
using System.Collections.Generic;
using System.Text;

using GenHTTP.Api.Protocol;

namespace GenHTTP.Api.Modules.Templating
{

    public class PageModel : IBaseModel
    {

        #region Get-/Setters

        public string? Title { get; set; }

        public IHttpRequest Request { get; }

        public IHttpResponse Response { get; }

        #endregion

        #region Functionality

        public PageModel(IHttpRequest request, IHttpResponse response)
        {
            Request = request;
            Response = response;
        }

        #endregion

    }

}
