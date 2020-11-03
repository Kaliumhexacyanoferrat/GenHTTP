using System.Net;

using GenHTTP.Api.Infrastructure;

using GenHTTP.Modules.IO;
using GenHTTP.Modules.LoadBalancing;

using Xunit;

namespace GenHTTP.Testing.Acceptance.Providers
{

    public class LoadBalancerTests
    {

        [Fact]
        public void TestProxy()
        {
            using var upstream = TestRunner.Run(Content.From(Resource.FromString("Proxy!")));

            var loadbalancer = LoadBalancer.Create()
                                           .Proxy($"http://localhost:{upstream.Port}");

            using var runner = TestRunner.Run(loadbalancer);

            using var response = runner.GetResponse();

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal("Proxy!", response.GetContent());                
        }

        [Fact]
        public void TestRedirect()
        {
            var loadbalancer = LoadBalancer.Create()
                                           .Redirect($"http://node");

            using var runner = TestRunner.Run(loadbalancer);

            using var response = runner.GetResponse("/page");

            Assert.Equal(HttpStatusCode.TemporaryRedirect, response.StatusCode);
            Assert.Equal("http://node/page", response.GetResponseHeader("Location"));
        }

        [Fact]
        public void TestCustomHandler()
        {
            var loadbalancer = LoadBalancer.Create()
                                           .Add(Content.From(Resource.FromString("My Content!")));

            using var runner = TestRunner.Run(loadbalancer);

            using var response = runner.GetResponse();

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal("My Content!", response.GetContent());
        }

        [Fact]
        public void TestPriorities()
        {
            var loadbalancer = LoadBalancer.Create()
                                           .Add(Content.From(Resource.FromString("Prio A")), r => Priority.High)
                                           .Add(Content.From(Resource.FromString("Prio B")), r => Priority.Low);

            using var runner = TestRunner.Run(loadbalancer);

            using var response = runner.GetResponse();

            Assert.Equal("Prio A", response.GetContent());
        }

        [Fact]
        public void TestMultiplePriorities()
        {
            var loadbalancer = LoadBalancer.Create()
                                           .Add(Content.From(Resource.FromString("Prio A1")), r => Priority.High)
                                           .Add(Content.From(Resource.FromString("Prio A2")), r => Priority.High)
                                           .Add(Content.From(Resource.FromString("Prio A3")), r => Priority.High);

            using var runner = TestRunner.Run(loadbalancer);

            using var response = runner.GetResponse();

            Assert.StartsWith("Prio A", response.GetContent());
        }
        
        [Fact]
        public void TestNoNodes()
        {
            using var runner = TestRunner.Run(LoadBalancer.Create());

            using var response = runner.GetResponse();

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

    }

}
