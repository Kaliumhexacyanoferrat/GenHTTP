using System;
using System.Collections.Generic;
using System.Text;

using GenHTTP.Api.Protocol;

namespace GenHTTP.Api.Modules.Templating
{

    public interface IBaseModel
    {
        
        IHttpRequest Request { get; }

        IHttpResponse Response { get; }

    }

}
