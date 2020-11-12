using System.Net;
using System.IO;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using GenHTTP.Api.Content;
using GenHTTP.Api.Protocol;

using GenHTTP.Modules.Controllers;
using GenHTTP.Modules.IO;
using GenHTTP.Modules.Layouting;

namespace GenHTTP.Testing.Acceptance.Modules.Controllers
{

    [TestClass]
    public class ActionTests
    {

        #region Supporting data structures

        public class Model
        {

            public string? Field { get; set; }

        }

        public class TestController
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
        public void TestIndex()
        {
            using var runner = GetRunner();

            using var response = runner.GetResponse("/t/");

            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            Assert.AreEqual("Hello World!", response.GetContent());
        }

        [TestMethod]
        public void TestAction()
        {
            using var runner = GetRunner();

            using var response = runner.GetResponse("/t/action/");

            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            Assert.AreEqual("Action", response.GetContent());
        }

        [TestMethod]
        public void TestActionWithQuery()
        {
            using var runner = GetRunner();

            using var response = runner.GetResponse("/t/action/?query=0815");

            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            Assert.AreEqual("815", response.GetContent());
        }

        [TestMethod]
        public void TestActionWithQueryFromBody()
        {
            using var runner = GetRunner();

            var request = runner.GetRequest("/t/action/");

            request.Method = "PUT";
            request.ContentType = "application/x-www-form-urlencoded";

            using (var input = new StreamWriter(request.GetRequestStream()))
            {
                input.Write("value2=test");
            }

            using var response = runner.GetResponse(request);

            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            Assert.AreEqual("Action test", response.GetContent());
        }

        [TestMethod]
        public void TestActionWithBody()
        {
            using var runner = GetRunner();

            var request = runner.GetRequest("/t/action/");
            request.Method = "POST";

            using (var input = new StreamWriter(request.GetRequestStream()))
            {
                input.Write("{ \"field\": \"FieldData\" }");
            }

            using var response = runner.GetResponse(request);

            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            Assert.AreEqual("FieldData", response.GetContent());
        }

        [TestMethod]
        public void TestActionWithParameter()
        {
            using var runner = GetRunner();

            using var response = runner.GetResponse("/t/simple-action/4711/");

            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            Assert.AreEqual("4711", response.GetContent());
        }

        [TestMethod]
        public void TestActionWithBadParameter()
        {
            using var runner = GetRunner();

            using var response = runner.GetResponse("/t/simple-action/string/");

            Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [TestMethod]
        public void TestActionWithMixedParameters()
        {
            using var runner = GetRunner();

            using var response = runner.GetResponse("/t/complex-action/1/2/?three=3");

            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            Assert.AreEqual("6", response.GetContent());
        }

        [TestMethod]
        public void TestActionWithNoResult()
        {
            using var runner = GetRunner();

            using var response = runner.GetResponse("/t/void/");

            Assert.AreEqual(HttpStatusCode.NoContent, response.StatusCode);
        }

        [TestMethod]
        public void TestNonExistingAction()
        {
            using var runner = GetRunner();

            using var response = runner.GetResponse("/t/nope/");

            Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
        }

        [TestMethod]
        public void TestHypenCasing()
        {
            using var runner = GetRunner();

            using var response = runner.GetResponse("/t/hypen-casing-99/");

            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            Assert.AreEqual("OK", response.GetContent());
        }

        [TestMethod]
        public void TestIndexController()
        {
            using var runner = TestRunner.Run(Layout.Create().IndexController<TestController>());

            using var response = runner.GetResponse("/simple-action/4711/");

            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            Assert.AreEqual("4711", response.GetContent());
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
