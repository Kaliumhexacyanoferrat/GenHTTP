using System;
using System.Collections.Generic;
using System.Text;

namespace GenHTTP.Core.Infrastructure
{

    internal class NetworkConfiguration
    {

        #region Get-/Setters

        internal TimeSpan RequestReadTimeout { get; }

        internal uint RequestMemoryLimit { get; }

        internal uint TransferBufferSize { get; }

        #endregion

        #region Initialization

        internal NetworkConfiguration(TimeSpan readTimeout, uint memoryLimit, uint transferBuffer)
        {
            RequestReadTimeout = readTimeout;
            RequestMemoryLimit = memoryLimit;
            TransferBufferSize = transferBuffer;
        }

        #endregion

    }

}
