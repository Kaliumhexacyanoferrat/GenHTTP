using System;
using System.Collections.Generic;
using System.Text;

using GenHTTP.Api.Protocol;
using GenHTTP.Content.Templating.Functions;

namespace GenHTTP.Content.Templating
{
    
    public class TemplatedTemplateViewModel
    {

        #region Get-/Setters

        public IHttpRequest Request { get; }

        public IHttpResponse Response { get; }

        public string Title { get; set; }

        public string Content { get; set; }
        
        #endregion

        #region Initialization

        public TemplatedTemplateViewModel(IHttpRequest request, IHttpResponse response)
        {
            Request = request;
            Response = response;

            Title = "";
            Content = "";

            Route = new RoutingMethod(Request.Routing);
        }

        #endregion

        #region Functionality

        public RoutingMethod Route { get; }

        #endregion

    }

}
