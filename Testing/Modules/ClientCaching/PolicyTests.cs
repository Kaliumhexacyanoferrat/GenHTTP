using System.Net.Http;
using System.Threading.Tasks;

using GenHTTP.Api.Protocol;

using GenHTTP.Modules.ClientCaching;
using GenHTTP.Modules.IO;
using GenHTTP.Modules.Layouting;
using GenHTTP.Modules.Sitemaps;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GenHTTP.Testing.Acceptance.Modules.ClientCaching
{

    [TestClass]
    public sealed class PolicyTests
    {

        [TestMethod]
        public async Task TestExpireHeaderSet()
        {
            var content = Content.From(Resource.FromString("Content"))
                                 .Add(ClientCache.Policy().Duration(1));

            using var runner = TestRunner.Run(content);

            using var response = await runner.GetResponse();

            Assert.IsNotNull(response.GetContentHeader("Expires"));
        }

        [TestMethod]
        public async Task TestExpireHeaderNotSetForOtherMethods()
        {
            var content = Content.From(Resource.FromString("Content"))
                                 .Add(ClientCache.Policy().Duration(1));

            using var runner = TestRunner.Run(content);

            var request = runner.GetRequest();
            request.Method = HttpMethod.Head;

            using var response = await runner.GetResponse(request);

            AssertX.IsNullOrEmpty(response.GetContentHeader("Expires"));
        }

        [TestMethod]
        public async Task TestExpireHeaderNotSetForOtherStatus()
        {
            var content = Layout.Create()
                                .Add(ClientCache.Policy().Duration(1));

            using var runner = TestRunner.Run(content);

            using var response = await runner.GetResponse();

            AssertX.IsNullOrEmpty(response.GetContentHeader("Expires"));
        }

        [TestMethod]
        public async Task TestPredicate()
        {
            var content = Content.From(Resource.FromString("Content"))
                                 .Add(ClientCache.Policy().Duration(1).Predicate((_, r) => r.ContentType?.RawType != "text/plain"));

            using var runner = TestRunner.Run(content);

            using var response = await runner.GetResponse();

            AssertX.IsNullOrEmpty(response.GetContentHeader("Expires"));
        }

        [TestMethod]
        public async Task TestContent()
        {
            var content = Layout.Create()
                                .Index(Content.From(Resource.FromString("Index").Type(new FlexibleContentType(ContentType.TextHtml))))
                                .Add(Sitemap.FILE_NAME, Sitemap.Create())
                                .Add(ClientCache.Policy().Duration(1));

            using var runner = TestRunner.Run(content);

            using var response = await runner.GetResponse("/" + Sitemap.FILE_NAME);

            Assert.AreEqual(1, (await response.GetSitemap()).Count);
        }

    }

}
