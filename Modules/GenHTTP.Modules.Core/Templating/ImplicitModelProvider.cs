using System;
using System.Collections.Generic;
using System.Text;

using GenHTTP.Api.Modules.Templating;
using GenHTTP.Api.Protocol;

namespace GenHTTP.Modules.Core.Templating
{

    public class ImplicitModelProvider : IPageProvider<PageModel>
    {

        public PageModel GetModel(IHttpRequest request, IHttpResponse response)
        {
            return new PageModel(request, response);
        }

    }

}
