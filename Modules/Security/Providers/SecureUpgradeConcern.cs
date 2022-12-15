using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using GenHTTP.Api.Content;
using GenHTTP.Api.Infrastructure;
using GenHTTP.Api.Protocol;

using GenHTTP.Modules.Basics;

namespace GenHTTP.Modules.Security.Providers
{

    public sealed class SecureUpgradeConcern : IConcern
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

        public async ValueTask<IResponse?> HandleAsync(IRequest request)
        {
            if (!request.EndPoint.Secure)
            {
                if (request.Server.EndPoints.Any(e => e.Secure))
                {
                    var endpoints = request.Server.EndPoints.Where(e => e.Secure)
                                                            .ToList();

                    if (endpoints.Count > 0)
                    {
                        if (Mode == SecureUpgrade.Force)
                        {
                            return await Redirect.To(GetRedirectLocation(request, endpoints))
                                                 .Build(this)
                                                 .HandleAsync(request)
                                                 .ConfigureAwait(false);
                        }
                        else if (Mode == SecureUpgrade.Allow)
                        {
                            if (request.Method.KnownMethod == RequestMethod.GET)
                            {
                                if (request.Headers.TryGetValue("Upgrade-Insecure-Requests", out var flag))
                                {
                                    if (flag == "1")
                                    {
                                        var response = await Redirect.To(GetRedirectLocation(request, endpoints), true)
                                                                     .Build(this)
                                                                     .HandleAsync(request)
                                                                     .ConfigureAwait(false);

                                        response?.Headers.Add("Vary", "Upgrade-Insecure-Requests");

                                        return response;
                                    }
                                }
                            }
                        }
                    }
                }
            }

            return await Content.HandleAsync(request).ConfigureAwait(false);
        }

        private static string GetRedirectLocation(IRequest request, List<IEndPoint> endPoints)
        {
            var targetPort = GetTargetPort(request, endPoints);

            var port = targetPort == 443 ? string.Empty : $":{targetPort}";

            return $"https://{request.HostWithoutPort()}{port}{request.Target.Path}";
        }

        private static ushort GetTargetPort(IRequest request, List<IEndPoint> endPoints)
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

        public ValueTask PrepareAsync() => Content.PrepareAsync();

        public IAsyncEnumerable<ContentElement> GetContentAsync(IRequest request) => Content.GetContentAsync(request);

        #endregion

    }

}
