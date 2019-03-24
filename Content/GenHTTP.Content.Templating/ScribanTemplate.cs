using System;
using System.Collections.Generic;
using System.Text;

using GenHTTP.Api.Content.Pages;
using GenHTTP.Api.Protocol;

namespace GenHTTP.Content.Templating
{

    public class ScribanTemplate : ITemplateProvider
    {

        #region Get-/Setters

        public string Template { get; }

        #endregion

        #region Initialization

        public ScribanTemplate(string template)
        {
            Template = template;
        }

        #endregion

        #region Functionality

        public IContentPage GetPage(IHttpRequest request, IHttpResponse response)
        {            
            return new ScribanPage(Template, new ScribanTemplateViewModel(request, response));
        }

        #endregion

    }

}
