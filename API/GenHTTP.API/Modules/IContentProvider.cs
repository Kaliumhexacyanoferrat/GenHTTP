using System;
using System.Collections.Generic;
using System.Text;

using GenHTTP.Api.Protocol;

namespace GenHTTP.Api.Modules
{
    
    public interface IContentProvider
    {

        void Handle(IHttpRequest request, IHttpResponse response);

    }

}
