using System;
using System.Collections.Generic;
using System.Text;

using GenHTTP.Api.Protocol;
using GenHTTP.Content.Templating.Functions;

namespace GenHTTP.Content.Templating
{

    public class TemplatedContentViewModel
    {

        #region Get-/Setters

        public string? Title { get; set; }

        public IHttpRequest Request { get; }

        public IHttpResponse Response { get; }

        #endregion

        #region Functionality 

        public TemplatedContentViewModel(IHttpRequest request, IHttpResponse response)
        {
            Request = request;
            Response = response;

            Route = new RoutingMethod(Request.Routing);
        }

        #endregion

        #region Functionality

        public RoutingMethod Route { get; }

        #endregion

    }
}
