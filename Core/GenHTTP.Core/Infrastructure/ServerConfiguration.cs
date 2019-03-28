using System;
using System.Collections.Generic;
using System.Text;

namespace GenHTTP.Core.Infrastructure
{

    internal class ServerConfiguration
    {

        #region Get-/Setters

        internal ushort Port { get; }

        internal ushort Backlog { get; }

        #endregion

        #region Initialization

        internal ServerConfiguration(ushort port, ushort backlog)
        {
            Port = port;
            Backlog = backlog;
        }

        #endregion

    }

}
