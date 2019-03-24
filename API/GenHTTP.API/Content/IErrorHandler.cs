using System;
using System.Collections.Generic;
using System.Text;

using GenHTTP.Api.Protocol;

namespace GenHTTP.Api.Content
{
    
    public interface IErrorHandler
    {

        IContentProvider GetContent(IHttpRequest request, IHttpResponse response);

    }

}
