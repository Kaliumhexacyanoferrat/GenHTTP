using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web;

using GenHTTP.Api.Content;
using GenHTTP.Api.Content.Templating;
using GenHTTP.Api.Protocol;
using GenHTTP.Modules.Basics;
using GenHTTP.Modules.IO;
using GenHTTP.Modules.IO.Streaming;

using PooledAwait;

namespace GenHTTP.Modules.ReverseProxy.Provider
{

    public class ReverseProxyProvider : IHandler
    {
        private static uint BUFFER_SIZE = 8192;

        private static readonly HashSet<string> RESERVED_RESPONSE_HEADERS = new()
        {
            "Server", "Date", "Content-Encoding", "Transfer-Encoding", "Content-Type",
            "Connection", "Content-Length", "Keep-Alive"
        };

        private static readonly HashSet<string> RESERVED_REQUEST_HEADERS = new()
        {
            "Host", "Connection", "Forwarded", "Upgrade-Insecure-Requests"
        };

        #region Get-/Setters

        public IHandler Parent { get; }

        public string Upstream { get; }

        public TimeSpan ConnectTimeout { get; }

        public TimeSpan ReadTimeout { get; }

        #endregion

        #region Initialization

        public ReverseProxyProvider(IHandler parent, string upstream, TimeSpan connectTimeout, TimeSpan readTimeout)
        {
            Parent = parent;
            Upstream = upstream;

            ConnectTimeout = connectTimeout;
            ReadTimeout = readTimeout;
        }

        #endregion

        #region Functionality

        public async ValueTask<IResponse?> HandleAsync(IRequest request)
        {
            try
            {
                var req = ConfigureRequest(request);

                if (request.Content != null && CanSendBody(request))
                {
                    using (var inputStream = await req.GetRequestStreamAsync().ConfigureAwait(false))
                    {
                        await request.Content.CopyPooledAsync(inputStream, BUFFER_SIZE).ConfigureAwait(false);
                    }
                }

                var resp = await GetSafeResponse(req).ConfigureAwait(false);

                return GetResponse(resp, request).Build();
            }
            catch (OperationCanceledException e)
            {
                var info = ContentInfo.Create()
                                      .Title("Gateway Timeout")
                                      .Build();

                return (await this.GetErrorAsync(new ErrorModel(request, this, ResponseStatus.GatewayTimeout, "The gateway did not respond in time.", e), info).ConfigureAwait(false)).Build();
            }
            catch (WebException e)
            {
                var info = ContentInfo.Create()
                                      .Title("Bad Gateway")
                                      .Build();

                return (await this.GetErrorAsync(new ErrorModel(request, this, ResponseStatus.BadGateway, "Unable to retrieve a response from the gateway.", e), info).ConfigureAwait(false)).Build();
            }
        }

        private HttpWebRequest ConfigureRequest(IRequest request)
        {
            var req = WebRequest.CreateHttp(GetRequestUri(request));

            req.AllowAutoRedirect = false;

            req.Timeout = (int)ConnectTimeout.TotalMilliseconds;
            req.ReadWriteTimeout = (int)ReadTimeout.TotalMilliseconds;

            req.AutomaticDecompression = DecompressionMethods.None;

            req.KeepAlive = true;
            req.Pipelined = true;

            req.Method = request.Method.RawMethod;

            foreach (var header in request.Headers)
            {
                if (!RESERVED_REQUEST_HEADERS.Contains(header.Key))
                {
                    req.Headers.Add(header.Key, header.Value);
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

        private string GetRequestUri(IRequest request)
        {
            return Upstream + request.Target.GetRemaining().ToString(true) + GetQueryString(request);
        }

        private string GetQueryString(IRequest request)
        {
            if (request.Query.Count > 0)
            {
                var query = HttpUtility.ParseQueryString(string.Empty);

                foreach (var kv in request.Query)
                {
                    query[kv.Key] = kv.Value;
                }

                return "?" + query?.ToString()?
                                   .Replace("+", "%20")
                                   .Replace("%2b", "+");
            }

            return string.Empty;
        }

        private IResponseBuilder GetResponse(HttpWebResponse response, IRequest request)
        {
            var builder = request.Respond();

            builder.Status((int)response.StatusCode, response.StatusDescription);

            foreach (var key in response.Headers.AllKeys)
            {
                if (!RESERVED_RESPONSE_HEADERS.Contains(key))
                {
                    var value = response.Headers[key];

                    if (value != null)
                    {
                        if (key == "Location")
                        {
                            builder.Header(key, RewriteLocation(value, request));
                        }
                        else if (key == "Set-Cookie")
                        {
                            foreach (var cookie in BrokenCookieHeaderParser.GetCookies(value))
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

            if (!string.IsNullOrEmpty(response.ContentEncoding))
            {
                builder.Encoding(response.ContentEncoding);
            }

            if (response.ContentLength > 0)
            {
                builder.Length((ulong)response.ContentLength);
            }

            if (HasBody(request, response))
            {
                builder.Content(response.GetResponseStream(), () => new ValueTask<ulong?>())
                       .Type(response.ContentType);
            }

            return builder;
        }

        private bool CanSendBody(IRequest request)
        {
            return !request.HasType(RequestMethod.GET, RequestMethod.HEAD, RequestMethod.OPTIONS);
        }

        private bool HasBody(IRequest request, HttpWebResponse response)
        {
            return !request.HasType(RequestMethod.HEAD) && response.ContentType != null;
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
                    relativePath = path.Substring(0, path.Length - scoped.Length);
                }
                else
                {
                    relativePath = path.Substring(1);
                }

                var protocol = request.EndPoint.Secure ? "https://" : "http://";

                return location.Replace(Upstream, protocol + request.Host + relativePath);
            }

            return location;
        }

        private async PooledValueTask<HttpWebResponse> GetSafeResponse(WebRequest request)
        {
            try
            {
                return (HttpWebResponse)await request.GetResponseAsync().ConfigureAwait(false);
            }
            catch (WebException e)
            {
                var response = e.Response as HttpWebResponse;

                if (response != null)
                {
                    return response;
                }
                else
                {
                    throw;
                }
            }
        }

        private string GetForwardings(IRequest request)
        {
            return string.Join(", ", request.Forwardings
                                            .Union(new[] { new Forwarding(request.LocalClient.IPAddress, request.LocalClient.Host, request.LocalClient.Protocol) })
                                            .Select(f => GetForwarding(f)));
        }

        private string GetForwarding(Forwarding forwarding)
        {
            var result = new List<string>(2);

            if (forwarding.For != null)
            {
                result.Add($"for={forwarding.For}");
            }

            if (forwarding.Host != null)
            {
                result.Add($"host={forwarding.Host}");
            }

            if (forwarding.Protocol != null)
            {
                result.Add($"proto={forwarding.Protocol?.ToString() ?? "http"}");
            }

            return string.Join("; ", result);
        }

        public IEnumerable<ContentElement> GetContent(IRequest request) => Enumerable.Empty<ContentElement>();

        #endregion

    }

}
