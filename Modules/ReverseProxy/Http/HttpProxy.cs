using System.Net;
using System.Net.Http.Headers;
using System.Net.Sockets;
using System.Web;

using GenHTTP.Api.Content;
using GenHTTP.Api.Protocol;
using GenHTTP.Modules.IO;
using GenHTTP.Modules.ReverseProxy.Websocket;

namespace GenHTTP.Modules.ReverseProxy.Http;

public sealed class HttpProxy : IHandler
{
    private static readonly ByteString UpgradeHeader = new("Upgrade");

    private static readonly ByteString WebsocketValue = new("websocket");

    private static readonly HashSet<string> ReservedResponseHeaders = new(StringComparer.OrdinalIgnoreCase)
    {
        "Server",
        "Date",
        "Content-Encoding",
        "Transfer-Encoding",
        "Content-Type",
        "Connection",
        "Content-Length",
        "Keep-Alive"
    };

    private static readonly HashSet<string> ReservedRequestHeaders = new(StringComparer.OrdinalIgnoreCase)
    {
        "Host",
        "Connection",
        "Forwarded",
        "Upgrade-Insecure-Requests"
    };

    #region Get-/Setters

    public string Upstream { get; }

    private HttpClient Client { get; }

    #endregion

    #region Initialization

    public HttpProxy(string upstream, HttpClient client)
    {
        Upstream = upstream;
        Client = client;
    }

    #endregion

    #region Functionality

    public async ValueTask<IResponse?> HandleAsync(IRequest request)
    {
        try
        {
            var upgradeHeader = request.Header.Headers.GetEntry(UpgradeHeader);

            if (upgradeHeader == WebsocketValue)
            {
                var wsProxy = new WebsocketProxy(Upstream);
                return await wsProxy.HandleAsync(request);
            }

            var req = ConfigureRequest(request);

            var resp = await Client.SendAsync(req, HttpCompletionOption.ResponseHeadersRead);

            return GetResponse(resp, request).Build();
        }
        catch (OperationCanceledException e)
        {
            throw new ProviderException(ResponseStatus.GatewayTimeout, "The gateway did not respond in time.", e);
        }
        catch (HttpRequestException e)
        {
            throw new ProviderException(ResponseStatus.BadGateway, "Unable to retrieve a response from the gateway.", e);
        }
    }

    private HttpRequestMessage ConfigureRequest(IRequest request)
    {
        var headers = request.Header.Headers;
        
        var req = new HttpRequestMessage(new HttpMethod(request.Header.Method.ToString()), GetRequestUri(request));

        for (var i = 0; i < headers.Count; i++)
        {
            var header = headers[i];

            var key = header.Key.ToString();
            var value = header.Value.ToString();

            if (!ReservedRequestHeaders.Contains(key))
            {
                if (key.StartsWith("Content-"))
                {
                    req.Content?.Headers.Add(key, value);
                }
                else
                {
                    req.Headers.Add(key, value);
                }
            }
        }

        // todo: req.Headers.Add("Forwarded", GetForwardings(request));

        /* todo: if (request.Cookies.Count > 0)
        {
            var cookieHeader = request.Cookies.Select(c => $"{c.Value.Name}={c.Value.Value}");

            req.Headers.Add("Cookie", string.Join("; ", cookieHeader));
        }*/

        var content = request.GetBody();

        if (content is not null && CanSendBody(request))
        {
            req.Content = new RequestBody(content);
        }

        return req;
    }

    private string GetRequestUri(IRequest request)
    {
        var remaining = request.Header.Target.AsString(decode: false, remainingOnly: true);

        if (!string.IsNullOrEmpty(remaining) && !remaining.StartsWith('/'))
        {
            remaining = '/' + remaining;
        }
        
        return Upstream + remaining + GetQueryString(request);
    } 

    private static string GetQueryString(IRequest request)
    {
        var query = request.Header.Query;

        if (query.Count > 0)
        {
            var queryBuilder = HttpUtility.ParseQueryString(string.Empty);

            for (var i = 0; i < query.Count; i++)
            {
                var arg = query[i];

                var key = arg.Key.ToString();
                var value= HttpUtility.UrlDecode(arg.Value.ToString());

                queryBuilder[key] = value;
            }

            return "?" + queryBuilder.ToString()?
                                     .Replace("+", "%20")
                                     .Replace("%2b", "+");
        }

        return string.Empty;
    }

    private IResponseBuilder GetResponse(HttpResponseMessage response, IRequest request)
    {
        var builder = request.Respond();

        builder.Status((ResponseStatus)response.StatusCode); // todo: rework status model

        SetHeaders(builder, request, response.Headers);

        var contentHeaders = response.Content.Headers;

        SetHeaders(builder, request, contentHeaders);

        if (HasBody(response))
        {
            builder.Content(new ClientResponseContent(response));
        }

        return builder;
    }

    private void SetHeaders(IResponseBuilder builder, IRequest request, HttpHeaders headers)
    {
        foreach (var (key, values) in headers)
        {
            if (!ReservedResponseHeaders.Contains(key))
            {
                var value = values.FirstOrDefault();

                if (value is not null)
                {
                    if (key.Equals("Location", StringComparison.OrdinalIgnoreCase))
                    {
                        builder.Header(key, RewriteLocation(value, request));
                    }
                    else if (key.Equals("Set-Cookie", StringComparison.OrdinalIgnoreCase))
                    {
                        foreach (var cookie in values)
                        {
                            builder.Header(key, cookie);
                        }
                    }
                    else
                    {
                        builder.Header(key, value);
                    }
                }
            }
        }
    }

    private static bool CanSendBody(IRequest request) => !request.HasType(RequestMethod.Get, RequestMethod.Head, RequestMethod.Options);

    private static bool HasBody(HttpResponseMessage response)
    {
        return response.Content.Headers.ContentType is not null && response.StatusCode != HttpStatusCode.NoContent;
    }

    private string RewriteLocation(string location, IRequest request)
    {
        if (location.StartsWith(Upstream))
        {
            var target = request.Header.Target;

            var path = target.AsString(decode: false);
            var scoped = target.AsString(decode: false, remainingOnly: true);

            string relativePath;

            if (scoped != "/" && !string.IsNullOrEmpty(scoped))
            {
                relativePath = path[..^scoped.Length];
            }
            else
            {
                relativePath = path[1..];
            }

            var protocol = request.EndPoint.Secure ? "https://" : "http://";

            var host = request.Header.Headers.GetEntry("Host");

            return location.Replace(Upstream, protocol + host + relativePath);
        }

        return location;
    }

    /*private static string GetForwardings(IRequest request)
    {
        return string.Join(", ", request.Forwardings
                                        .Union([
                                            new Forwarding(request.LocalClient.IPAddress, request.LocalClient.Host, request.LocalClient.Protocol)
                                        ])
                                        .Select(GetForwarding));
    }*/

    private static string GetForwarding(Forwarding forwarding)
    {
        var result = new List<string>(2);

        if (forwarding.For is not null)
        {
            if (forwarding.For.AddressFamily == AddressFamily.InterNetworkV6)
            {
                result.Add($"for=[{forwarding.For}]");
            }
            else
            {
                result.Add($"for={forwarding.For}");
            }
        }

        if (forwarding.Host is not null)
        {
            result.Add($"host={forwarding.Host}");
        }

        if (forwarding.Protocol is not null)
        {
            result.Add($"proto={forwarding.Protocol?.ToString() ?? "http"}");
        }

        return string.Join("; ", result);
    }

    public ValueTask PrepareAsync() => ValueTask.CompletedTask;

    #endregion

}
