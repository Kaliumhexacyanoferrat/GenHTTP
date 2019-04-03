using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace GenHTTP.Api.Infrastructure
{
    
    public interface IEndPoint
    {

        IPAddress IPAddress { get; }

        ushort Port { get; }

        bool Secure { get; }
    
    }

}
