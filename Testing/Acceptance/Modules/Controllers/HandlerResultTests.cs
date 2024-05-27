using System.Net;
using System.Threading.Tasks;

using GenHTTP.Api.Content;
using GenHTTP.Api.Content.IO;
using GenHTTP.Api.Infrastructure;

using GenHTTP.Modules.Controllers;
using GenHTTP.Modules.IO;
using GenHTTP.Modules.Layouting;
using GenHTTP.Modules.StaticWebsites;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GenHTTP.Testing.Acceptance.Modules.Controllers
{

    [TestClass]
    public sealed class HandlerResultTests
    {

        #region Supporting data structures

        public sealed class TestController
        {
            private static readonly IBuilder<IResourceTree> _Tree = CreateTree();

            public IHandlerBuilder Website()
            {
                return StaticWebsite.From(_Tree);
            }

            private static IBuilder<IResourceTree> CreateTree()
            {
                var subTree = VirtualTree.Create()
                                         .Add("index.htm", Resource.FromString("Sub Index"))
                                         .Add("my.txt", Resource.FromString("My Textfile"));

                return VirtualTree.Create()
                                  .Add("index.html", Resource.FromString("Index"))
                                  .Add("sub", subTree);
            }

        }

        #endregion

        #region Tests

        [TestMethod]
        public async Task TestHandlerRouting()
        {
            var app = Layout.Create()
                            .AddController<TestController>("c");

            using var host = TestHost.Run(app);

            using var response = await host.GetResponseAsync("/c/website/sub/my.txt");

            await response.AssertStatusAsync(HttpStatusCode.OK);

            Assert.AreEqual("My Textfile", await response.GetContentAsync());
        }

        #endregion

    }

}
