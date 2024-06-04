using System;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using GenHTTP.Api.Content;

using GenHTTP.Modules.Basics;
using GenHTTP.Modules.Controllers;
using GenHTTP.Modules.IO;
using GenHTTP.Modules.Layouting;
using System.Threading.Tasks;

namespace GenHTTP.Testing.Acceptance.Modules.Controllers
{

    [TestClass]
    public sealed class RoutingTests
    {

        #region Supporting data structures

        public sealed class RouteController
        {

            public IHandlerBuilder InnerController([FromPath] int i)
            {
                return Layout.Create()
                             .Add((i + 1).ToString(), Controller.From<RouteController>());
            }

            public IHandlerBuilder Index() => Content.From(Resource.FromString("Index"));

            public IHandlerBuilder DoSomethingWithController()
            {
                return Redirect.To("{controller}/", true);
            }

            public IHandlerBuilder DoSomethingWithIndex()
            {
                return Redirect.To("{index}/", true);
            }

            public IHandlerBuilder DoSomethingWithParent()
            {
                return Redirect.To("{layout}/test", true);
            }

            public IHandlerBuilder DoSomethingWithAppenders()
            {
                return Redirect.To("appenders/1/2/", true);
            }

        }

        #endregion

        #region Tests

        [TestMethod]
        public async Task TestAppenders()
        {
            using var runner = Setup();

            using var response = await runner.GetResponseAsync("/r/appenders/1/test/");

            Assert.AreEqual("/r/appenders/1/test/", await response.GetContentAsync());
        }

        [TestMethod]
        public async Task TestNested()
        {
            using var runner = Setup();

            using var response = await runner.GetResponseAsync("/r/nested/1/test/inner");

            Assert.AreEqual("/r/nested/1/test/inner", await response.GetContentAsync());
        }

        /// <summary>
        /// Ensure that nesting of controllers is possible and
        /// routing still works as expected.
        /// </summary>
        [TestMethod]
        public async Task TestInner()
        {
            using var runner = Setup();

            using var response = await runner.GetResponseAsync("/r/inner-controller/1/2/inner-controller/3/4/appenders/5/6/");

            Assert.AreEqual("/r/inner-controller/1/2/inner-controller/3/4/appenders/5/6/", await response.GetContentAsync());
        }

        [TestMethod]
        public async Task TestRoutingToController()
        {
            using var runner = Setup();

            using var response = await runner.GetResponseAsync("/r/do-something-with-controller/");

            Assert.AreEqual("/r/", new Uri(response.GetHeader("Location")!).AbsolutePath);
        }

        [TestMethod]
        public async Task TestRoutingToIndex()
        {
            using var runner = Setup();

            using var response = await runner.GetResponseAsync("/r/do-something-with-index/");

            Assert.AreEqual("/r/", new Uri(response.GetHeader("Location")!).AbsolutePath);
        }

        [TestMethod]
        public async Task TestRoutingToParent()
        {
            using var runner = Setup();

            using var response = await runner.GetResponseAsync("/r/do-something-with-parent/");

            Assert.AreEqual("/test", new Uri(response.GetHeader("Location")!).AbsolutePath);
        }

        [TestMethod]
        public async Task TestRoutingToAppender()
        {
            using var runner = Setup();

            using var response = await runner.GetResponseAsync("/r/do-something-with-appenders/");

            Assert.AreEqual("/r/appenders/1/2/", new Uri(response.GetHeader("Location")!).AbsolutePath);
        }

        #endregion

        #region Helpers

        private TestHost Setup()
        {
            var layout = Layout.Create()
                               .AddController<RouteController>("r")
                               .Add(Content.From(Resource.FromString("Blubb")));

            return TestHost.Run(layout);
        }

        #endregion

    }

}
