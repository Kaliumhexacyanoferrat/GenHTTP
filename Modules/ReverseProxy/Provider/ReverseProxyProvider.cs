using System.Globalization;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Sockets;
using System.Web;

using GenHTTP.Api.Content;
using GenHTTP.Api.Protocol;

using GenHTTP.Modules.IO;

namespace GenHTTP.Modules.ReverseProxy.Provider;

public sealed class ReverseProxyProvider : IHandler
{
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

    public ReverseProxyProvider(string upstream, HttpClient client)
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
        var req = new HttpRequestMessage(new HttpMethod(request.Method.RawMethod), GetRequestUri(request));

        if (request.Content is not null && CanSendBody(request))
        {
            req.Content = new StreamContent(request.Content);
        }

        foreach (var header in request.Headers)
        {
            if (!ReservedRequestHeaders.Contains(header.Key))
            {
                if (header.Key.StartsWith("Content-"))
                {
                    req.Content?.Headers.Add(header.Key, header.Value);
                }
                else
                {
                    req.Headers.Add(header.Key, header.Value);
                }
            }
        }

        req.Headers.Add("Forwarded", GetForwardings(request));

        if (request.Cookies.Count > 0)
        {
            var cookieHeader = request.Cookies.Select(c => $"{c.Value.Name}={c.Value.Value}");

            req.Headers.Add("Cookie", string.Join("; ", cookieHeader));
        }

        return req;
    }

    private string GetRequestUri(IRequest request) => Upstream + request.Target.GetRemaining().ToString(true) + GetQueryString(request);

    private static string GetQueryString(IRequest request)
    {
        if (request.Query.Count > 0)
        {
            var query = HttpUtility.ParseQueryString(string.Empty);

            foreach (var kv in request.Query)
            {
                query[kv.Key] = kv.Value;
            }

            return "?" + query.ToString()?
                              .Replace("+", "%20")
                              .Replace("%2b", "+");
        }

        return string.Empty;
    }

    private IResponseBuilder GetResponse(HttpResponseMessage response, IRequest request)
    {
        var builder = request.Respond();

        builder.Status((int)response.StatusCode, response.ReasonPhrase ?? "No Reason");

        SetHeaders(builder, request, response.Headers);

        var contentHeaders = response.Content.Headers;

        SetHeaders(builder, request, contentHeaders);

        if (contentHeaders.ContentEncoding.Count > 0)
        {
            builder.Encoding(contentHeaders.ContentEncoding.First());
        }

        ulong? knownLength = contentHeaders.ContentLength > 0 ? (ulong)contentHeaders.ContentLength : null;

        if (HasBody(request, response))
        {
            builder.Content(new ClientResponseContent(response))
                   .Type(contentHeaders.ContentType?.ToString() ?? "application/octet-stream");
        }
        else
        {
            if (request.HasType(RequestMethod.Head) && (knownLength != null))
            {
                builder.Length(knownLength.Value);
            }

            response.Dispose();
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
                    if (key == "Location")
                    {
                        builder.Header(key, RewriteLocation(value, request));
                    }
                    else if (key == "Expires")
                    {
                        if (TryParseDate(value, out var expires))
                        {
                            builder.Expires(expires);
                        }
                    }
                    else if (key == "Last-Modified")
                    {
                        if (TryParseDate(value, out var modified))
                        {
                            builder.Modified(modified);
                        }
                    }
                    else if (key == "Set-Cookie")
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

    private static bool HasBody(IRequest request, HttpResponseMessage response)
    {
        return !request.HasType(RequestMethod.Head)
            && response.Content.Headers.ContentType is not null
            && response.StatusCode != HttpStatusCode.NoContent;
    }

    private string RewriteLocation(string location, IRequest request)
    {
        if (location.StartsWith(Upstream))
        {
            var path = request.Target.Path.ToString();
            var scoped = request.Target.GetRemaining().ToString();

            string relativePath;

            if (scoped != "/")
            {
                relativePath = path[..^scoped.Length];
            }
            else
            {
                relativePath = path[1..];
            }

            var protocol = request.EndPoint.Secure ? "https://" : "http://";

            return location.Replace(Upstream, protocol + request.Host + relativePath);
        }

        return location;
    }

    private static string GetForwardings(IRequest request)
    {
        return string.Join(", ", request.Forwardings
                                        .Union([
                                            new Forwarding(request.LocalClient.IpAddress, request.LocalClient.Host, request.LocalClient.Protocol)
                                        ])
                                        .Select(GetForwarding));
    }

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

    private static bool TryParseDate(string value, out DateTime parsedValue) => DateTime.TryParseExact(value, CultureInfo.InvariantCulture.DateTimeFormat.RFC1123Pattern, CultureInfo.InvariantCulture, DateTimeStyles.AllowWhiteSpaces | DateTimeStyles.AssumeUniversal, out parsedValue);

    public ValueTask PrepareAsync() => ValueTask.CompletedTask;

    #endregion

}
