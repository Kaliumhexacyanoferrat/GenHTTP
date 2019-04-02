using System;
using System.Collections.Generic;
using System.Text;

using GenHTTP.Api.Protocol;

namespace GenHTTP.Api.Modules.Templating
{

    public delegate T ModelProvider<T>(IRequest request);

    public interface IBaseModel
    {
        
        IRequest Request { get; }
        
    }

}
