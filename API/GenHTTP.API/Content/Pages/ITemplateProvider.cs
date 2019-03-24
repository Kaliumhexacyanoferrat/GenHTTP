using System;
using System.Collections.Generic;
using System.Text;

using GenHTTP.Api.Protocol;

namespace GenHTTP.Api.Content.Pages
{

    public interface ITemplateProvider
    {

        IContentPage GetPage(IHttpRequest request, IHttpResponse response);

    }

}
