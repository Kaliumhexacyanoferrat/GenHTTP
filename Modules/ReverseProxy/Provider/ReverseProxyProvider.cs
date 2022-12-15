using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Web;

using GenHTTP.Api.Content;
using GenHTTP.Api.Content.Templating;
using GenHTTP.Api.Protocol;

using GenHTTP.Modules.Basics;
using GenHTTP.Modules.Pages;

namespace GenHTTP.Modules.ReverseProxy.Provider
{

    public sealed class ReverseProxyProvider : IHandler
    {
        private static readonly HashSet<string> RESERVED_RESPONSE_HEADERS = new(StringComparer.OrdinalIgnoreCase)
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

        private static readonly HashSet<string> RESERVED_REQUEST_HEADERS = new(StringComparer.OrdinalIgnoreCase)
        {
            "Host",
            "Connection",
            "Forwarded",
            "Upgrade-Insecure-Requests"
        };

        #region Get-/Setters

        public IHandler Parent { get; }

        public string Upstream { get; }

        private HttpClient Client { get; }

        #endregion

        #region Initialization

        public ReverseProxyProvider(IHandler parent, string upstream, TimeSpan connectTimeout, TimeSpan readTimeout)
        {
            Parent = parent;
            Upstream = upstream;

            var handler = new SocketsHttpHandler
            {
                AllowAutoRedirect = false,
                AutomaticDecompression = DecompressionMethods.None,
                ConnectTimeout = connectTimeout
            };

            Client = new HttpClient(handler)
            {
                Timeout = readTimeout
            };
        }

        #endregion

        #region Functionality

        public async ValueTask<IResponse?> HandleAsync(IRequest request)
        {
            try
            {
                var req = ConfigureRequest(request);

                var resp = await Client.SendAsync(req, HttpCompletionOption.ResponseHeadersRead).ConfigureAwait(false);

                return GetResponse(resp, request).Build();
            }
            catch (OperationCanceledException e)
            {
                var info = ContentInfo.Create()
                                      .Title("Gateway Timeout")
                                      .Build();

                return this.GetError(new ErrorModel(request, this, ResponseStatus.GatewayTimeout, "The gateway did not respond in time.", e), info).Build();
            }
            catch (HttpRequestException e)
            {
                var info = ContentInfo.Create()
                                      .Title("Bad Gateway")
                                      .Build();

                return this.GetError(new ErrorModel(request, this, ResponseStatus.BadGateway, "Unable to retrieve a response from the gateway.", e), info).Build();
            }
        }

        private HttpRequestMessage ConfigureRequest(IRequest request)
        {
            var req = new HttpRequestMessage(new(request.Method.RawMethod), GetRequestUri(request));

            if (request.Content is not null && CanSendBody(request))
            {
                req.Content = new StreamContent(request.Content);
            }

            foreach (var header in request.Headers)
            {
                if (!RESERVED_REQUEST_HEADERS.Contains(header.Key))
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

        private string GetRequestUri(IRequest request)
        {
            return Upstream + request.Target.GetRemaining().ToString(true) + GetQueryString(request);
        }

        private static string GetQueryString(IRequest request)
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

        private IResponseBuilder GetResponse(HttpResponseMessage response, IRequest request)
        {
            var builder = request.Respond();

            builder.Status((int)response.StatusCode, response.ReasonPhrase ?? "No Reason");

            SetHeaders(builder, request, response.Headers);

            var contentHeaders = (response.Content.Headers);

            SetHeaders(builder, request, contentHeaders);

            if (contentHeaders.ContentEncoding.Any())
            {
                builder.Encoding(contentHeaders.ContentEncoding.First());
            }

            ulong? knownLength = (contentHeaders.ContentLength > 0) ? (ulong)contentHeaders.ContentLength : null;

            if (HasBody(request, response))
            {
                builder.Content(new ClientResponseContent(response))
                       .Type(contentHeaders.ContentType?.ToString() ?? "application/octet-stream");
            }
            else
            {
                if (knownLength != null)
                {
                    builder.Length(knownLength.Value);
                }

                response.Dispose();
            }

            return builder;
        }

        private void SetHeaders(IResponseBuilder builder, IRequest request, HttpHeaders headers)
        {
            foreach (var kv in headers)
            {
                var key = kv.Key;

                if (!RESERVED_RESPONSE_HEADERS.Contains(key))
                {
                    var value = kv.Value.First();

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
                            foreach (var cookie in kv.Value)
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

        private static bool CanSendBody(IRequest request)
        {
            return !request.HasType(RequestMethod.GET, RequestMethod.HEAD, RequestMethod.OPTIONS);
        }

        private static bool HasBody(IRequest request, HttpResponseMessage response)
        {
            return !request.HasType(RequestMethod.HEAD) && response.Content.Headers.ContentType is not null;
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
                                            .Union(new[] { new Forwarding(request.LocalClient.IPAddress, request.LocalClient.Host, request.LocalClient.Protocol) })
                                            .Select(f => GetForwarding(f)));
        }

        private static string GetForwarding(Forwarding forwarding)
        {
            var result = new List<string>(2);

            if (forwarding.For is not null)
            {
                result.Add($"for={forwarding.For}");
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

        private static bool TryParseDate(string value, out DateTime parsedValue)
        {
            return DateTime.TryParseExact(value, CultureInfo.InvariantCulture.DateTimeFormat.RFC1123Pattern, CultureInfo.InvariantCulture, DateTimeStyles.AllowWhiteSpaces | DateTimeStyles.AssumeUniversal, out parsedValue);
        }

        public ValueTask PrepareAsync() => ValueTask.CompletedTask;

        public IAsyncEnumerable<ContentElement> GetContentAsync(IRequest request) => AsyncEnumerable.Empty<ContentElement>();

        #endregion

    }

}
