using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Linq;
using System.Web;

using Net = System.Net;

using GenHTTP.Api.Modules;
using GenHTTP.Api.Protocol;
using System.Net.Http;

namespace GenHTTP.Modules.Core.Proxy
{

    public class ReverseProxyProvider : IContentProvider
    {
        private static readonly HashSet<string> RESERVED_RESPONSE_HEADERS = new HashSet<string>
        {
            "Server", "Date", "Content-Encoding", "Transfer-Encoding", "Content-Type",
            "Connection", "Content-Length", "Keep-Alive"
        };

        private static readonly HashSet<string> RESERVED_REQUEST_HEADERS = new HashSet<string>
        {
            "Host", "Connection"
        };

        #region Get-/Setters

        public string Upstream { get; }

        public TimeSpan ConnectTimeout { get; }

        public TimeSpan ReadTimeout { get; }

        #endregion

        #region Initialization

        public ReverseProxyProvider(string upstream, TimeSpan connectTimeout, TimeSpan readTimeout)
        {
            Upstream = upstream;

            ConnectTimeout = connectTimeout;
            ReadTimeout = readTimeout;
        }

        #endregion

        #region Functionality

        public IResponseBuilder Handle(IRequest request)
        {
            try
            {
                var req = ConfigureRequest(request);

                if (request.Content != null)
                {
                    using (var inputStream = req.GetRequestStream())
                    {
                        request.Content.CopyTo(inputStream);
                    }
                }
                
                var resp = GetSafeResponse(req);
                
                return GetResponse(resp, request);
            }
            catch (OperationCanceledException e)
            {
                return request.Respond(ResponseStatus.GatewayTimeout, e);
            }
            catch (WebException e)
            {
                return request.Respond(ResponseStatus.BadGateway, e);
            }
        }

        private string GetRequestUri(IRequest request)
        {
            var path = request.Routing?.ScopedPath ?? throw new InvalidOperationException("No routing context available");

            return Upstream + path + GetQueryString(request);
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

            if (request.Cookies.Count > 0)
            {
                var cookieHeader = request.Cookies.Values
                                                  .Select(c => $"{c.Name}={c.Value}");

                req.Headers.Add("Cookie", string.Join("; ", cookieHeader));
            }

            return req;
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
                            builder.Cookie(cookie);
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
                builder.ContentLength((ulong)response.ContentLength);
            }

            if (!request.HasType(RequestMethod.HEAD))
            {
                builder.Content(response.GetResponseStream(), response.ContentType);
            }

            return builder;
        }

        private string RewriteLocation(string location, IRequest request)
        {
            if (location.StartsWith(Upstream))
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

            return location;
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

        #endregion

    }

}
