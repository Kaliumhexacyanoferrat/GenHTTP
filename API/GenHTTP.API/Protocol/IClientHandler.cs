using GenHTTP.Api.Infrastructure;
using System;
using System.Net;

namespace GenHTTP.Api.Protocol
{
    
    public interface IClientHandler
    {

        /// <summary>
        /// The server this handler relates to.
        /// </summary>
        IServer Server { get; }

        /// <summary>
        /// The IP of the connected client.
        /// </summary>
        IPAddress IPAddress { get; }

        /// <summary>
        /// Time span between handling the request and sending the response.
        /// </summary>
        TimeSpan? LoadTime { get; }

    }

}
