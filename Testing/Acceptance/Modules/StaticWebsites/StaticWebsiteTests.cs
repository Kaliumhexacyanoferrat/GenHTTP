using System.Net;
using System.Threading.Tasks;

using GenHTTP.Modules.IO;
using GenHTTP.Modules.StaticWebsites;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GenHTTP.Testing.Acceptance.Modules.StaticWebsites;

[TestClass]
public sealed class StaticWebsiteTests
{

    [TestMethod]
    public async Task TestWithIndex()
    {
            var tree = VirtualTree.Create()
                                  .Add("index.html", Resource.FromString("Index 1"))
                                  .Add("sub", VirtualTree.Create().Add("index.htm", Resource.FromString("Index 2")));

            using var runner = TestHost.Run(StaticWebsite.From(tree));

            using var indexResponse = await runner.GetResponseAsync();
            Assert.AreEqual("Index 1", await indexResponse.GetContentAsync());

            using var subIndexResponse = await runner.GetResponseAsync("/sub/");
            Assert.AreEqual("Index 2", await subIndexResponse.GetContentAsync());
        }

    [TestMethod]
    public async Task TestNoIndex()
    {
            var tree = VirtualTree.Create()
                                  .Add("sub", VirtualTree.Create());

            using var runner = TestHost.Run(StaticWebsite.From(tree));

            using var indexResponse = await runner.GetResponseAsync();
            await indexResponse.AssertStatusAsync(HttpStatusCode.NotFound);

            using var subIndexResponse = await runner.GetResponseAsync("/sub/"); 
            await subIndexResponse.AssertStatusAsync(HttpStatusCode.NotFound);
        }

}
