using GenHTTP.Api.Protocol;
using System;
using System.Collections.Generic;
using System.Text;

namespace GenHTTP.Api.Modules.Templating
{

    public class TemplateModel : IBaseModel
    {

        #region Get-/Setters

        public string Title { get; }

        public string Content { get; }

        public IHttpRequest Request { get; }

        public IHttpResponse Response { get; }

        #endregion

        #region Initialization

        public TemplateModel(IHttpRequest request, IHttpResponse response, string title, string content)
        {
            Title = title;
            Content = content;

            Request = request;
            Response = response;
        }

        #endregion

    }

}

