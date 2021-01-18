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
        public void TestExpireHeaderSet()
        {
            var content = Content.From(Resource.FromString("Content"))
                                 .Add(ClientCache.Policy().Duration(1));

            using var runner = TestRunner.Run(content);

            using var response = runner.GetResponse();

            Assert.IsNotNull(response.GetResponseHeader("Expires"));
        }

        [TestMethod]
        public void TestExpireHeaderNotSetForOtherMethods()
        {
            var content = Content.From(Resource.FromString("Content"))
                                 .Add(ClientCache.Policy().Duration(1));

            using var runner = TestRunner.Run(content);

            var request = runner.GetRequest();
            request.Method = "HEAD";

            using var response = runner.GetResponse(request);

            Assert.AreEqual(string.Empty, response.GetResponseHeader("Expires"));
        }

        [TestMethod]
        public void TestExpireHeaderNotSetForOtherStatus()
        {
            var content = Layout.Create()
                                .Add(ClientCache.Policy().Duration(1));

            using var runner = TestRunner.Run(content);

            using var response = runner.GetResponse();

            Assert.AreEqual(string.Empty, response.GetResponseHeader("Expires"));
        }

        [TestMethod]
        public void TestPredicate()
        {
            var content = Content.From(Resource.FromString("Content"))
                                 .Add(ClientCache.Policy().Duration(1).Predicate((_, r) => r.ContentType?.RawType != "text/plain"));

            using var runner = TestRunner.Run(content);

            using var response = runner.GetResponse();

            Assert.AreEqual(string.Empty, response.GetResponseHeader("Expires"));
        }

        [TestMethod]
        public void TestContent()
        {
            var content = Layout.Create()
                                .Index(Content.From(Resource.FromString("Index").Type(new FlexibleContentType(ContentType.TextHtml))))
                                .Add(Sitemap.FILE_NAME, Sitemap.Create())
                                .Add(ClientCache.Policy().Duration(1));

            using var runner = TestRunner.Run(content);

            using var response = runner.GetResponse("/" + Sitemap.FILE_NAME);

            Assert.AreEqual(1, response.GetSitemap().Count);
        }

    }

}
