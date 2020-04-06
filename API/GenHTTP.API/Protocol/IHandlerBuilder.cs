using System;
using System.Collections.Generic;
using System.Text;

namespace GenHTTP.Api.Protocol
{
    
    public interface IHandlerBuilder
    {

        IHandler Build(IHandler parent);

    }

}
