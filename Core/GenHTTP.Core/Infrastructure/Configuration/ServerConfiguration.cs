using System;
using System.Collections.Generic;
using System.Text;

namespace GenHTTP.Core.Infrastructure.Configuration
{

    internal class ServerConfiguration
    {

        #region Get-/Setters

        internal IEnumerable<EndPointConfiguration> EndPoints { get; }

        internal NetworkConfiguration Network { get; }

        #endregion

        #region Initialization

        internal ServerConfiguration(IEnumerable<EndPointConfiguration> endPoints, NetworkConfiguration networkConfiguration)
        {
            EndPoints = endPoints;
            Network = networkConfiguration;
        }

        #endregion

    }

}
