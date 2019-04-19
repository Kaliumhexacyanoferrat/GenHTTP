using System;
using System.Collections.Generic;
using System.Text;

using GenHTTP.Api.Infrastructure;
using GenHTTP.Api.Modules;
using GenHTTP.Api.Protocol;

namespace GenHTTP.Modules.Core.Security
{
    
    public class StrictTransportExtension : IServerExtension
    {
        private const string HEADER = "Strict-Transport-Security";

        #region Get-/Setters

        public StrictTransportPolicy Policy { get; }

        private string HeaderValue { get; }

        #endregion

        #region Initialization

        public StrictTransportExtension(StrictTransportPolicy policy)
        {
            Policy = policy;
            HeaderValue = GetPolicyHeader();
        }

        #endregion

        #region Functionality

        public IContentProvider? Intercept(IRequest request) => null;

        public void Intercept(IRequest request, IResponse response)
        {
            if (request.EndPoint.Secure)
            {
                if (!response.Headers.ContainsKey(HEADER))
                {
                    response[HEADER] = HeaderValue;
                }
            }
        }

        private string GetPolicyHeader()
        {
            var seconds = (int)Policy.MaximumAge.TotalSeconds;

            var result = $"max-age={seconds}";

            if (Policy.IncludeSubdomains)
            {
                result += "; includeSubDomains";
            }

            if (Policy.Preload)
            {
                result += "; preload";
            }

            return result;
        }

        #endregion
        
    }

}
