using GenHTTP.Api.Protocol;
using System;
using System.Collections.Generic;
using System.Text;

namespace GenHTTP.Api.Content
{
    
    public interface IContentProvider
    {

        void Handle(IHttpRequest request, IHttpResponse response);

    }

}
