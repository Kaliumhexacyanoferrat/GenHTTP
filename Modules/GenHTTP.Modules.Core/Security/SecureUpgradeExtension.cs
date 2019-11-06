using System;
using System.Linq;

using GenHTTP.Api.Infrastructure;
using GenHTTP.Api.Modules;
using GenHTTP.Api.Protocol;

namespace GenHTTP.Modules.Core.Security
{

    public class SecureUpgradeExtension : IServerExtension
    {

        #region Get-/Setters

        public SecureUpgrade Mode { get; }

        #endregion

        #region Initialization

        public SecureUpgradeExtension(SecureUpgrade mode)
        {
            Mode = mode;
        }

        #endregion

        #region Functionality

        public IContentProvider? Intercept(IRequest request)
        {
            if (!request.EndPoint.Secure)
            {
                if (Mode == SecureUpgrade.Force)
                {
                    return Redirect.To(GetRedirectLocation(request))
                                   .Build();
                }
                else if (Mode == SecureUpgrade.Allow)
                {
                    if (request.Method.KnownMethod == RequestMethod.GET)
                    {
                        if (request.Headers.TryGetValue("Upgrade-Insecure-Requests", out var flag))
                        {
                            if (flag == "1")
                            {
                                return Redirect.To(GetRedirectLocation(request), true)
                                               .Modify(r => r.Header("Vary", "Upgrade-Insecure-Requests"))
                                               .Build();
                            }
                        }
                    }
                }
            }

            return null;
        }

        public void Intercept(IRequest request, IResponse response)
        {
            // reponse does not need to get modified
        }

        private string GetRedirectLocation(IRequest request)
        {
            var targetPort = GetTargetPort(request);

            var port = (targetPort == 443) ? string.Empty : $":{targetPort}";

            return $"https://{request.HostWithoutPort()}{port}{request.Path}";
        }

        private ushort GetTargetPort(IRequest request)
        {
            var endpoints = request.Server.EndPoints.Where(e => e.Secure).ToList();

            // this extension can only be added if there are secure endpoints available
            if (endpoints.Count == 0)
            {
                throw new NotSupportedException("No secure endpoints available");
            }

            // if there is a correlated port, use this one
            var correlated = (ushort)(request.EndPoint.Port + (443 - 80));

            if (endpoints.Any(e => e.Port == correlated))
            {
                return correlated;
            }

            // default to 443, if available
            if (endpoints.Any(e => e.Port == 443))
            {
                return 443;
            }

            // use the first secure endpoint
            return endpoints.First().Port;
        }

        #endregion

    }

}
