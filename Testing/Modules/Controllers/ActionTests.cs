﻿using System.Net;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using GenHTTP.Api.Content;
using GenHTTP.Api.Protocol;

using GenHTTP.Modules.Controllers;
using GenHTTP.Modules.IO;
using GenHTTP.Modules.Layouting;
using System.Collections.Generic;
using System.Net.Http.Headers;

namespace GenHTTP.Testing.Acceptance.Modules.Controllers
{

    [TestClass]
    public sealed class ActionTests
    {

        #region Supporting data structures

        public sealed class Model
        {

            public string? Field { get; set; }

        }

        public sealed class TestController
        {

            public IHandlerBuilder Index()
            {
                return Content.From(Resource.FromString("Hello World!"));
            }

            public IHandlerBuilder Action(int? query)
            {
                return Content.From(Resource.FromString(query?.ToString() ?? "Action"));
            }

            [ControllerAction(RequestMethod.PUT)]
            public IHandlerBuilder Action(int? value1, string value2)
            {
                return Content.From(Resource.FromString((value1?.ToString() ?? "Action") + $" {value2}"));
            }

            public IHandlerBuilder SimpleAction([FromPath] int id)
            {
                return Content.From(Resource.FromString(id.ToString()));
            }

            public IHandlerBuilder ComplexAction(int three, [FromPath] int one, [FromPath] int two)
            {
                return Content.From(Resource.FromString((one + two + three).ToString()));
            }

            [ControllerAction(RequestMethod.POST)]
            public IHandlerBuilder Action(Model data)
            {
                return Content.From(Resource.FromString(data.Field ?? "no content"));
            }

            public IHandlerBuilder HypenCAsing99()
            {
                return Content.From(Resource.FromString("OK"));
            }

            public void Void() { }

        }

        #endregion

        #region Tests

        [TestMethod]
        public async Task TestIndex()
        {
            using var runner = GetRunner();

            using var response = await runner.GetResponse("/t/");

            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            Assert.AreEqual("Hello World!", await response.GetContent());
        }

        [TestMethod]
        public async Task TestAction()
        {
            using var runner = GetRunner();

            using var response = await runner.GetResponse("/t/action/");

            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            Assert.AreEqual("Action", await response.GetContent());
        }

        [TestMethod]
        public async Task TestActionWithQuery()
        {
            using var runner = GetRunner();

            using var response = await runner.GetResponse("/t/action/?query=0815");

            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            Assert.AreEqual("815", await response.GetContent());
        }

        [TestMethod]
        public async Task TestActionWithQueryFromBody()
        {
            using var runner = GetRunner();

            var dict = new Dictionary<string, string>()
            {
                { "value2", "test" }
            };

            var request = runner.GetRequest("/t/action/");

            request.Method = HttpMethod.Put;
            request.Content = new FormUrlEncodedContent(dict);

            using var response = await runner.GetResponse(request);

            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            Assert.AreEqual("Action test", await response.GetContent());
        }

        [TestMethod]
        public async Task TestActionWithBody()
        {
            using var runner = GetRunner();

            var request = runner.GetRequest("/t/action/");

            request.Method = HttpMethod.Post;

            request.Content = new StringContent("{ \"field\": \"FieldData\" }", null, "application/json");
            request.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

            using var response = await runner.GetResponse(request);

            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            Assert.AreEqual("FieldData", await response.GetContent());
        }

        [TestMethod]
        public async Task TestActionWithParameter()
        {
            using var runner = GetRunner();

            using var response = await runner.GetResponse("/t/simple-action/4711/");

            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            Assert.AreEqual("4711", await response.GetContent());
        }

        [TestMethod]
        public async Task TestActionWithBadParameter()
        {
            using var runner = GetRunner();

            using var response = await runner.GetResponse("/t/simple-action/string/");

            Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [TestMethod]
        public async Task TestActionWithMixedParameters()
        {
            using var runner = GetRunner();

            using var response = await runner.GetResponse("/t/complex-action/1/2/?three=3");

            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            Assert.AreEqual("6", await response.GetContent());
        }

        [TestMethod]
        public async Task TestActionWithNoResult()
        {
            using var runner = GetRunner();

            using var response = await runner.GetResponse("/t/void/");

            Assert.AreEqual(HttpStatusCode.NoContent, response.StatusCode);
        }

        [TestMethod]
        public async Task TestNonExistingAction()
        {
            using var runner = GetRunner();

            using var response = await runner.GetResponse("/t/nope/");

            Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
        }

        [TestMethod]
        public async Task TestHypenCasing()
        {
            using var runner = GetRunner();

            using var response = await runner.GetResponse("/t/hypen-casing-99/");

            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            Assert.AreEqual("OK", await response.GetContent());
        }

        [TestMethod]
        public async Task TestIndexController()
        {
            using var runner = TestRunner.Run(Layout.Create().IndexController<TestController>());

            using var response = await runner.GetResponse("/simple-action/4711/");

            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            Assert.AreEqual("4711", await response.GetContent());
        }

        #endregion

        #region Helpers

        private TestRunner GetRunner()
        {
            return TestRunner.Run(Layout.Create().AddController<TestController>("t"));
        }

        #endregion

    }

}
