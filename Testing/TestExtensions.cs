using System;
using System.Linq;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Security;
using System.Threading.Tasks;
using System.Security.Cryptography.X509Certificates;
using System.Xml;
using System.Xml.Linq;
using System.Xml.XPath;

using GenHTTP.Api.Content;

using GenHTTP.Testing.Acceptance.Utilities;

namespace GenHTTP.Testing.Acceptance
{

    public static class TestExtensions
    {

        #region Supporting data structures

        public class HandlerBuilder : IHandlerBuilder<HandlerBuilder>
        {
            private readonly IHandler _Handler;

            private readonly List<IConcernBuilder> _Concerns = new();

            public HandlerBuilder(IHandler handler) { _Handler = handler; }

            public HandlerBuilder Add(IConcernBuilder concern)
            {
                _Concerns.Add(concern);
                return this;
            }

            public IHandler Build(IHandler parent)
            {
                if (_Handler is FunctionalHandler func)
                {
                    func.Parent = parent;
                }

                return Concerns.Chain(parent, _Concerns, (p) => _Handler);
            }

        }

        #endregion

        public static string GetContent(this HttpWebResponse response)
        {
            using var stream = response.GetResponseStream();
            using var reader = new StreamReader(stream);

            return reader.ReadToEnd();
        }

        public static HashSet<string> GetSitemap(this HttpWebResponse response)
        {
            var content = response.GetContent();

            var sitemap = XDocument.Parse(content);

            var namespaces = new XmlNamespaceManager(new NameTable());

            namespaces.AddNamespace("n", "http://www.sitemaps.org/schemas/sitemap/0.9");

            return sitemap.Root?.XPathSelectElements("//n:loc", namespaces)
                                .Select(x => new Uri(x.Value).AbsolutePath)
                                .ToHashSet() ?? new HashSet<string>();
        }

        public static HttpWebResponse GetSafeResponse(this WebRequest request)
        {
            try
            {
                return (HttpWebResponse)request.GetResponse();
            }
            catch (WebException e)
            {
                var response = e.Response as HttpWebResponse;

                if (response is not null)
                {
                    return response;
                }
                else
                {
                    throw;
                }
            }
        }

        public static async Task<HttpWebResponse> GetSafeResponseAsync(this WebRequest request)
        {
            try
            {
                return (HttpWebResponse)await request.GetResponseAsync();
            }
            catch (WebException e)
            {
                var response = e.Response as HttpWebResponse;

                if (response is not null)
                {
                    return response;
                }
                else
                {
                    throw;
                }
            }
        }

        public static DateTime WithoutMS(this DateTime date)
        {
            return new DateTime(date.Year, date.Month, date.Day, date.Hour, date.Minute, date.Second, date.Kind);
        }

        public static void IgnoreSecurityErrors(this HttpWebRequest request)
        {
            request.ServerCertificateValidationCallback = (object sender, X509Certificate? certificate, X509Chain? chain, SslPolicyErrors sslPolicyErrors) =>
            {
                return true;
            };
        }

        public static IHandlerBuilder<HandlerBuilder> Wrap(this IHandler handler) => new HandlerBuilder(handler);

    }

}
