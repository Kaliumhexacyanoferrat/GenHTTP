using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace GenHTTP.Api.Infrastructure
{
    
    public interface IClient
    {

        IPAddress IPAddress { get; }

        ushort Port { get; }

    }

}
