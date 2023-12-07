using System.Net;
using System.Threading.Tasks;

using GenHTTP.Api.Infrastructure;

using GenHTTP.Modules.IO;
using GenHTTP.Modules.LoadBalancing;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GenHTTP.Testing.Acceptance.Providers
{

    [TestClass]
    public sealed class LoadBalancerTests
    {

        [TestMethod]
        public async Task TestProxy()
        {
            using var upstream = TestRunner.Run(Content.From(Resource.FromString("Proxy!")));

            var loadbalancer = LoadBalancer.Create()
                                           .Proxy($"http://localhost:{upstream.Port}");

            using var runner = TestRunner.Run(loadbalancer);

            using var response = await runner.GetResponse();

            await response.AssertStatusAsync(HttpStatusCode.OK);
            Assert.AreEqual("Proxy!", await response.GetContent());                
        }

        [TestMethod]
        public async Task TestRedirect()
        {
            var loadbalancer = LoadBalancer.Create()
                                           .Redirect($"http://node");

            using var runner = TestRunner.Run(loadbalancer);

            using var response = await runner.GetResponse("/page");

            await response.AssertStatusAsync(HttpStatusCode.TemporaryRedirect);
            Assert.AreEqual("http://node/page", response.GetHeader("Location"));
        }

        [TestMethod]
        public async Task TestCustomHandler()
        {
            var loadbalancer = LoadBalancer.Create()
                                           .Add(Content.From(Resource.FromString("My Content!")));

            using var runner = TestRunner.Run(loadbalancer);

            using var response = await runner.GetResponse();

            await response.AssertStatusAsync(HttpStatusCode.OK);
            Assert.AreEqual("My Content!", await response.GetContent());
        }

        [TestMethod]
        public async Task TestPriorities()
        {
            var loadbalancer = LoadBalancer.Create()
                                           .Add(Content.From(Resource.FromString("Prio A")), r => Priority.High)
                                           .Add(Content.From(Resource.FromString("Prio B")), r => Priority.Low);

            using var runner = TestRunner.Run(loadbalancer);

            using var response = await runner.GetResponse();

            Assert.AreEqual("Prio A", await response.GetContent());
        }

        [TestMethod]
        public async Task TestMultiplePriorities()
        {
            var loadbalancer = LoadBalancer.Create()
                                           .Add(Content.From(Resource.FromString("Prio A1")), r => Priority.High)
                                           .Add(Content.From(Resource.FromString("Prio A2")), r => Priority.High)
                                           .Add(Content.From(Resource.FromString("Prio A3")), r => Priority.High);

            using var runner = TestRunner.Run(loadbalancer);

            using var response = await runner.GetResponse();

            AssertX.StartsWith("Prio A", await response.GetContent());
        }
        
        [TestMethod]
        public async Task TestNoNodes()
        {
            using var runner = TestRunner.Run(LoadBalancer.Create());

            using var response = await runner.GetResponse();

            await response.AssertStatusAsync(HttpStatusCode.NotFound);
        }

    }

}
