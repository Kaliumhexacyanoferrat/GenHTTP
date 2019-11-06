using System;

namespace GenHTTP.Core.Infrastructure.Configuration
{

    internal class NetworkConfiguration
    {

        #region Get-/Setters

        internal TimeSpan RequestReadTimeout { get; }

        internal uint RequestMemoryLimit { get; }

        internal uint TransferBufferSize { get; }

        internal ushort Backlog { get; }

        #endregion

        #region Initialization

        internal NetworkConfiguration(TimeSpan readTimeout, uint memoryLimit, uint transferBuffer, ushort backlog)
        {
            RequestReadTimeout = readTimeout;
            RequestMemoryLimit = memoryLimit;
            TransferBufferSize = transferBuffer;
            Backlog = backlog;
        }

        #endregion

    }

}
