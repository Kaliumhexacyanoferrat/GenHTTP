using GenHTTP.Api.Content;
using System;
using System.Collections.Generic;
using System.Text;

namespace GenHTTP.Api.Infrastructure
{

    public interface IServer
    {

        /// <summary>
        /// You can subscribe to this event if you want to get notified whenever a request was sucessfully handled by a project.
        /// </summary>
        event RequestHandled OnRequestHandled;

        /// <summary>
        /// The server will call this method every TimerIntervall seconds.
        /// </summary>
        event TimerTick OnTimerTick;

        /// <summary>
        /// The version of the server software.
        /// </summary>
        string Version { get; }

        /// <summary>
        /// The log file handler of the server.
        /// </summary>
        ILog Log { get; }

        /// <summary>
        /// Retrieve the path the server is running in.
        /// </summary>
        string Path { get; }

        /// <summary>
        /// All available projects running on this server.
        /// </summary>
        IProjectCollection Projects { get; }

        /// <summary>
        /// Specifies, whether the server is currently congested or not. It the server has to many
        /// connections, it will enable the burst mode, which closes all keep-alive connections.
        /// </summary>
        bool Congested { get; }

        /// <summary>
        /// The configuration of the server (changeable during runtime).
        /// </summary>
        Configuration Configuration { get; }

        IServerPage NewPage();

        IContentProvider DefaultNotFoundProvider { get; }

        IContentProvider DefaultNotLoggedInProvider { get; }

        IContentProvider DefaultNotEnoughRightsProvider { get; }

        IContentProvider DefaultWrongParametersProvider { get; }

    }

}
