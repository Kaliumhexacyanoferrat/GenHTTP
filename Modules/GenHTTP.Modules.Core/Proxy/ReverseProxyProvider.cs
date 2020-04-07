using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;

using GenHTTP.Api.Content;
using GenHTTP.Api.Protocol;
using GenHTTP.Modules.Core.General;

namespace GenHTTP.Modules.Core.Proxy
{

    public class ReverseProxyProvider : IHandler
    {
        private static readonly HashSet<string> RESERVED_RESPONSE_HEADERS = new HashSet<string>
        {
            "Server", "Date", "Content-Encoding", "Transfer-Encoding", "Content-Type",
            "Connection", "Content-Length", "Keep-Alive"
        };

        private static readonly HashSet<string> RESERVED_REQUEST_HEADERS = new HashSet<string>
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

        public IResponse Handle(IRequest request)
        {
            try
            {
                var req = ConfigureRequest(request);

                if ((request.Content != null) && CanSendBody(request))
                {
                    using (var inputStream = req.GetRequestStream())
                    {
                        request.Content.CopyTo(inputStream);
                    }
                }

                var resp = GetSafeResponse(req);

                return GetResponse(resp, request).Build();
            }
            catch (OperationCanceledException) // e)
            {
                return request.Respond()
                              .Status(ResponseStatus.GatewayTimeout)
                              .Build();
                // ToDo: return request.Respond(ResponseStatus.GatewayTimeout, e).Build();
            }
            catch (WebException) // e)
            {
                return request.Respond()
                              .Status(ResponseStatus.BadGateway)
                              .Build();
                // ToDo: return request.Respond(ResponseStatus.BadGateway, e).Build();
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
            //var path = request.Routing?.ScopedPath ?? throw new InvalidOperationException("No routing context available");

            //return Upstream + path + GetQueryString(request);

            return string.Empty; // Todo!
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

                return "?" + query.ToString();
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
                    if (key == "Location")
                    {
                        builder.Header(key, RewriteLocation(response.Headers[key], request));
                    }
                    else if (key == "Set-Cookie")
                    {
                        foreach (var cookie in BrokenCookieHeaderParser.GetCookies(response.Headers[key]))
                        {
                            builder.Header(key, cookie);
                        }
                    }
                    else
                    {
                        builder.Header(key, response.Headers[key]);
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
                builder.Content(response.GetResponseStream())
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
            return !request.HasType(RequestMethod.HEAD) && (response.ContentType != null);
        }

        private string RewriteLocation(string location, IRequest request)
        {
            return location; // ToDo!

            /*if (location.StartsWith(Upstream))
            {
                var routing = request.Routing ?? throw new InvalidOperationException("No routing context available");

                string relativePath;

                if (routing.ScopedPath != "/")
                {
                    relativePath = request.Path.Substring(0, request.Path.Length - routing.ScopedPath.Length);
                }
                else
                {
                    relativePath = request.Path.Substring(1);
                }

                var protocol = request.EndPoint.Secure ? "https://" : "http://";

                return location.Replace(Upstream, protocol + request.Host + relativePath);
            }

            return location;*/
        }

        private HttpWebResponse GetSafeResponse(WebRequest request)
        {
            try
            {
                return (HttpWebResponse)request.GetResponse();
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
                result.Add($"proto={forwarding.Protocol}");
            }

            return string.Join("; ", result);
        }

        public IEnumerable<ContentElement> GetContent(IRequest request)
        {
            // we cannot tell the content available on the target site
            return new List<ContentElement>();
        }

        #endregion

    }

}
