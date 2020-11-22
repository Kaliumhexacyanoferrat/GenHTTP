using System.Net;

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
        public void TestProxy()
        {
            using var upstream = TestRunner.Run(Content.From(Resource.FromString("Proxy!")));

            var loadbalancer = LoadBalancer.Create()
                                           .Proxy($"http://localhost:{upstream.Port}");

            using var runner = TestRunner.Run(loadbalancer);

            using var response = runner.GetResponse();

            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            Assert.AreEqual("Proxy!", response.GetContent());                
        }

        [TestMethod]
        public void TestRedirect()
        {
            var loadbalancer = LoadBalancer.Create()
                                           .Redirect($"http://node");

            using var runner = TestRunner.Run(loadbalancer);

            using var response = runner.GetResponse("/page");

            Assert.AreEqual(HttpStatusCode.TemporaryRedirect, response.StatusCode);
            Assert.AreEqual("http://node/page", response.GetResponseHeader("Location"));
        }

        [TestMethod]
        public void TestCustomHandler()
        {
            var loadbalancer = LoadBalancer.Create()
                                           .Add(Content.From(Resource.FromString("My Content!")));

            using var runner = TestRunner.Run(loadbalancer);

            using var response = runner.GetResponse();

            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            Assert.AreEqual("My Content!", response.GetContent());
        }

        [TestMethod]
        public void TestPriorities()
        {
            var loadbalancer = LoadBalancer.Create()
                                           .Add(Content.From(Resource.FromString("Prio A")), r => Priority.High)
                                           .Add(Content.From(Resource.FromString("Prio B")), r => Priority.Low);

            using var runner = TestRunner.Run(loadbalancer);

            using var response = runner.GetResponse();

            Assert.AreEqual("Prio A", response.GetContent());
        }

        [TestMethod]
        public void TestMultiplePriorities()
        {
            var loadbalancer = LoadBalancer.Create()
                                           .Add(Content.From(Resource.FromString("Prio A1")), r => Priority.High)
                                           .Add(Content.From(Resource.FromString("Prio A2")), r => Priority.High)
                                           .Add(Content.From(Resource.FromString("Prio A3")), r => Priority.High);

            using var runner = TestRunner.Run(loadbalancer);

            using var response = runner.GetResponse();

            AssertX.StartsWith("Prio A", response.GetContent());
        }
        
        [TestMethod]
        public void TestNoNodes()
        {
            using var runner = TestRunner.Run(LoadBalancer.Create());

            using var response = runner.GetResponse();

            Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
        }

    }

}
