using System;
using System.Collections.Generic;
using System.Text;

using GenHTTP.Api.Modules.Templating;
using GenHTTP.Api.Protocol;

namespace GenHTTP.Modules.Core.Templating
{

    public class ImplicitModelProvider : IPageProvider<PageModel>
    {

        public PageModel GetModel(IRequest request)
        {
            return new PageModel(request);
        }

    }

}
