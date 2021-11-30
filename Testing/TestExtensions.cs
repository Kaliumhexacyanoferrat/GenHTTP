using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
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

        public static async ValueTask<string> GetContent(this HttpResponseMessage response) => await response.Content.ReadAsStringAsync();

        public static string? GetETag(this HttpResponseMessage response) => response.GetHeader("ETag");

        public static string? GetHeader(this HttpResponseMessage response, string key)
        {
            if (response.Headers.TryGetValues(key, out var values))
            {
                return values.FirstOrDefault();
            }

            return null;
        }

        public static string? GetContentHeader(this HttpResponseMessage response, string key)
        {
            if (response.Content.Headers.TryGetValues(key, out var values))
            {
                return values.FirstOrDefault();
            }

            return null;
        }

        public static async Task<HashSet<string>> GetSitemap(this HttpResponseMessage response)
        {
            var content = await response.GetContent();

            var sitemap = XDocument.Parse(content);

            var namespaces = new XmlNamespaceManager(new NameTable());

            namespaces.AddNamespace("n", "http://www.sitemaps.org/schemas/sitemap/0.9");

            return sitemap.Root?.XPathSelectElements("//n:loc", namespaces)
                                .Select(x => new Uri(x.Value).AbsolutePath)
                                .ToHashSet() ?? new HashSet<string>();
        }

        public static DateTime WithoutMS(this DateTime date)
        {
            return new DateTime(date.Year, date.Month, date.Day, date.Hour, date.Minute, date.Second, date.Kind);
        }

        public static IHandlerBuilder<HandlerBuilder> Wrap(this IHandler handler) => new HandlerBuilder(handler);

    }

}
