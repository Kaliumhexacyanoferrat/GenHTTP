using System;
using System.Collections.Generic;
using System.Linq;

using GenHTTP.Api.Content;
using GenHTTP.Api.Infrastructure;
using GenHTTP.Api.Protocol;

namespace GenHTTP.Modules.Core.Security
{

    public class SecureUpgradeConcern : IConcern
    {

        #region Get-/Setters

        public SecureUpgrade Mode { get; }

        public IHandler Parent { get; }

        public IHandler Content { get; }

        #endregion

        #region Initialization

        public SecureUpgradeConcern(IHandler parent, Func<IHandler, IHandler> contentFactory, SecureUpgrade mode)
        {
            Parent = parent;
            Content = contentFactory(this);

            Mode = mode;
        }

        #endregion

        #region Functionality

        public IResponse? Handle(IRequest request)
        {
            if (!request.EndPoint.Secure)
            {
                var endpoints = request.Server.EndPoints.Where(e => e.Secure)
                                                        .ToList();

                if (endpoints.Count > 0)
                {
                    if (Mode == SecureUpgrade.Force)
                    {
                        return Redirect.To(GetRedirectLocation(request, endpoints))
                                       .Build(this)
                                       .Handle(request);
                    }
                    else if (Mode == SecureUpgrade.Allow)
                    {
                        if (request.Method.KnownMethod == RequestMethod.GET)
                        {
                            if (request.Headers.TryGetValue("Upgrade-Insecure-Requests", out var flag))
                            {
                                if (flag == "1")
                                {
                                    var response = Redirect.To(GetRedirectLocation(request, endpoints), true)
                                                           .Build(this)
                                                           .Handle(request)!;

                                    response.Headers.Add("Vary", "Upgrade-Insecure-Requests");

                                    return response;
                                }
                            }
                        }
                    }
                }
            }

            return Content.Handle(request);
        }

        private string GetRedirectLocation(IRequest request, List<IEndPoint> endPoints)
        {
            var targetPort = GetTargetPort(request, endPoints);

            var port = (targetPort == 443) ? string.Empty : $":{targetPort}";

            return $"https://{request.HostWithoutPort()}{port}{request.Target.Path}";
        }

        private ushort GetTargetPort(IRequest request, List<IEndPoint> endPoints)
        {
            // this extension can only be added if there are secure endpoints available
            if (endPoints.Count == 0)
            {
                throw new NotSupportedException("No secure endpoints available");
            }

            // if there is a correlated port, use this one
            var correlated = (ushort)(request.EndPoint.Port + (443 - 80));

            if (endPoints.Any(e => e.Port == correlated))
            {
                return correlated;
            }

            // default to 443, if available
            if (endPoints.Any(e => e.Port == 443))
            {
                return 443;
            }

            // use the first secure endpoint
            return endPoints.First().Port;
        }

        public IEnumerable<ContentElement> GetContent(IRequest request) => Content.GetContent(request);

        #endregion

    }

}
