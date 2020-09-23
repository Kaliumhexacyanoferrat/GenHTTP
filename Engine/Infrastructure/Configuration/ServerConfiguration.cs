using System.Collections.Generic;

namespace GenHTTP.Engine.Infrastructure.Configuration
{

    internal class ServerConfiguration
    {

        #region Get-/Setters

        internal IEnumerable<EndPointConfiguration> EndPoints { get; }

        internal NetworkConfiguration Network { get; }

        internal bool DevelopmentMode { get; }

        #endregion

        #region Initialization

        internal ServerConfiguration(bool developmentMode, IEnumerable<EndPointConfiguration> endPoints,
                                     NetworkConfiguration networkConfiguration)
        {
            DevelopmentMode = developmentMode;

            EndPoints = endPoints;
            Network = networkConfiguration;
        }

        #endregion

    }

}
