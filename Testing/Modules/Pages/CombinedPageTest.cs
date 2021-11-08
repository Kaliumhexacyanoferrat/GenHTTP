using System.Threading.Tasks;

using GenHTTP.Api.Content.Templating;
using GenHTTP.Modules.IO;
using GenHTTP.Modules.Markdown;
using GenHTTP.Modules.Pages;
using GenHTTP.Modules.Razor;
using GenHTTP.Modules.Scriban;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GenHTTP.Testing.Acceptance.Modules.Pages
{

    [TestClass]
    public class CombinedPageTest
    {

        [TestMethod]
        public void TestMetaData()
        {
            var page = CombinedPage.Create()
                                   .Title("My Page")
                                   .Description("My Description");

            using var runner = TestRunner.Run(page);

            using var response = runner.GetResponse();

            var content = response.GetContent();

            AssertX.Contains("My Page", content);
            AssertX.Contains("My Description", content);

            Assert.AreEqual("text/html", response.GetResponseHeader("Content-Type"));
        }

        [TestMethod]
        public void TestPlainText()
        {
            var page = CombinedPage.Create()
                                   .Add("Static Content");

            using var runner = TestRunner.Run(page);

            using var response = runner.GetResponse();

            var content = response.GetContent();

            AssertX.Contains("Static Content", content);
        }

        [TestMethod]
        public void TestRenderingEngines()
        {
            static ValueTask<BasicModel> model(Api.Protocol.IRequest r, Api.Content.IHandler h) => new(new BasicModel(r, h));

            var page = CombinedPage.Create()
                                   .AddScriban(Resource.FromString("Scriban at {{ request.target.path }}"), model)
                                   .AddRazor(Resource.FromString("Razor at @Model.Request.Target.Path"), model)
                                   .AddMarkdown(Resource.FromString("Mark*down*"));

            using var runner = TestRunner.Run(page);

            using var response = runner.GetResponse("/page");

            var content = response.GetContent();

            AssertX.Contains("Scriban at /page", content);
            AssertX.Contains("Razor at /page", content);
            AssertX.Contains("Mark<em>down</em>", content);
        }

        [TestMethod]
        public void TestStableChecksum()
        {
            var page = CombinedPage.Create()
                                   .Add("Static Content");

            using var runner = TestRunner.Run(page);

            using var r1 = runner.GetResponse();
            using var r2 = runner.GetResponse();

            Assert.IsNotNull(r1.GetResponseHeader("ETag"));

            Assert.AreEqual(r1.GetResponseHeader("ETag"), r2.GetResponseHeader("ETag"));
        }

    }

}
