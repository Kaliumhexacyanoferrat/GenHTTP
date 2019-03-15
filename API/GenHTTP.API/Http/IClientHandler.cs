using GenHTTP.Api.Infrastructure;
using System;
using System.Collections.Generic;
using System.Text;

namespace GenHTTP.Api.Http
{
    
    public interface IClientHandler
    {

        /// <summary>
        /// The server this handler relates to.
        /// </summary>
        IServer Server { get; }

        /// <summary>
        /// Time span between handling the request and sending the response.
        /// </summary>
        double LoadTime { get; }

        /// <summary>
        /// The IP of the connected client.
        /// </summary>
        string IP { get; }

        /// <summary>
        /// The port of the connected client.
        /// </summary>
        int Port { get; }
        
    }

}
