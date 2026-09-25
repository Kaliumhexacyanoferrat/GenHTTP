using System.Text;

using GenHTTP.Api.Content;
using GenHTTP.Api.Infrastructure;
using GenHTTP.Api.Protocol;

using GenHTTP.Modules.Redirects;
using GenHTTP.Modules.IO;

namespace GenHTTP.Modules.Security.Providers;

public sealed class SecureUpgradeConcern : IConcern
{
    private static readonly ByteString UpgradeInsecureRequestsHeader = new("Upgrade-Insecure-Requests");

    private static readonly ByteString VaryHeader = new("Vary");

    private static readonly ByteString YesValue = new("1");

    private static readonly byte[] AcmeChallengePrefix = "/.well-known/acme-challenge/"u8.ToArray();

    #region Get-/Setters

    public SecureUpgrade Mode { get; }

    public IHandler Content { get; }

    #endregion

    #region Initialization

    public SecureUpgradeConcern(IHandler content, SecureUpgrade mode)
    {
        Content = content;

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
                    if (Mode == SecureUpgrade.Force && !IsAcmeChallenge(request))
                    {
                        return await Redirect.To(GetRedirectLocation(request, endpoints))
                                             .Build()
                                             .HandleAsync(request);
                    }
                    if (Mode == SecureUpgrade.Allow)
                    {
                        if (request.Header.Method == RequestMethod.Get)
                        {
                            var flag = request.Header.Headers.GetEntry(UpgradeInsecureRequestsHeader);

                            if (flag == YesValue)
                            {
                                var response = await Redirect.To(GetRedirectLocation(request, endpoints), true)
                                                             .Build()
                                                             .HandleAsync(request);

                                response?.Rebuild().Header(VaryHeader, UpgradeInsecureRequestsHeader);

                                return response;
                            }
                        }
                    }
                }
            }
        }

        return await Content.HandleAsync(request);
    }

    private static string GetRedirectLocation(IRequest request, List<IEndPoint> endPoints)
    {
        var targetPort = GetTargetPort(request, endPoints);

        var port = targetPort == 443 ? string.Empty : $":{targetPort}";

        var host = request.GetHostWithoutPort();

        var hostString = (host != null) ? host.Value.ToString() : string.Empty;

        var location = new StringBuilder();

        location.Append("https://")
                .Append(hostString)
                .Append(port)
                .Append(request.Header.Path.ToString());

        AppendQuery(location, request.Header.Query);

        return location.ToString();
    }

    private static void AppendQuery(StringBuilder location, IRequestQuery query)
    {
        for (var i = 0; i < query.Count; i++)
        {
            var entry = query.GetStringEntry(i);

            location.Append(i == 0 ? '?' : '&')
                    .Append(entry.Key.ToString());

            if (!entry.Value.Bytes.IsEmpty)
            {
                location.Append('=')
                        .Append(entry.Value.ToString());
            }
        }
    }

    private static bool IsAcmeChallenge(IRequest request)
        => request.Header.Method == RequestMethod.Get
           && request.Header.Path.Bytes.Span.StartsWith(AcmeChallengePrefix);

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

    public ValueTask PrepareAsync(IServer server) => Content.PrepareAsync(server);

    #endregion

}
