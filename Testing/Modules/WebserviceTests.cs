using System;
using System.IO;
using System.Net;
using System.Text;
using System.Xml.Serialization;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using GenHTTP.Api.Content;
using GenHTTP.Api.Protocol;

using GenHTTP.Modules.IO;
using GenHTTP.Modules.Webservices;
using GenHTTP.Modules.Basics;
using GenHTTP.Modules.Layouting;
using System.Threading.Tasks;

namespace GenHTTP.Testing.Acceptance.Modules.Webservices
{

    [TestClass]
    public sealed class WebserviceTests
    {

        #region Supporting structures

        public sealed class TestEntity
        {

            public int ID { get; set; }

            public double? Nullable { get; set; }

        }

        public enum TestEnum
        {
            One,
            Two
        }

        public sealed class TestResource
        {

            [ResourceMethod("nothing")]
            public void DoNothing() { }

            [ResourceMethod("primitive")]
            public int Primitive(int input) => input;

            [ResourceMethod("guid")]
            public Guid Guid(Guid id) => id;

            [ResourceMethod(RequestMethod.POST, "entity")]
            public TestEntity Entity(TestEntity entity) => entity;

            [ResourceMethod(RequestMethod.PUT, "stream")]
            public Stream Stream(Stream input) => new MemoryStream(Encoding.UTF8.GetBytes(input.Length.ToString()));

            [ResourceMethod("requestResponse")]
            public ValueTask<IResponse?> RequestResponse(IRequest request)
            {
                return request.Respond()
                              .Content("Hello World")
                              .Type(ContentType.TextPlain)
                              .BuildTask();
            }

            [ResourceMethod("exception")]
            public void Exception() => throw new ProviderException(ResponseStatus.AlreadyReported, "Already reported!");

            [ResourceMethod("duplicate")]
            public void Duplicate1() { }

            [ResourceMethod("duplicate")]
            public void Duplicate2() { }

            [ResourceMethod("param/:param")]
            public int PathParam(int param) => param;

            [ResourceMethod("regex/(?<param>[0-9]+)")]
            public int RegexParam(int param) => param;

            [ResourceMethod]
            public void Empty() { }

            [ResourceMethod("enum")]
            public TestEnum Enum(TestEnum input) => input;

            [ResourceMethod("nullable")]
            public int? Nullable(int? input) => input;

            [ResourceMethod("request")]
            public string? Request(IHandler handler, IRequest request) => "yes";

        }

        #endregion

        #region Tests

        [TestMethod]
        public void TestEmpty()
        {
            WithResponse("", r => Assert.AreEqual(HttpStatusCode.NoContent, r.StatusCode));
        }

        [TestMethod]
        public void TestVoidReturn()
        {
            WithResponse("nothing", r => Assert.AreEqual(HttpStatusCode.NoContent, r.StatusCode));
        }

        [TestMethod]
        public void TestPrimitives()
        {
            WithResponse("primitive?input=42", r => Assert.AreEqual("42", r.GetContent()));
        }

        [TestMethod]
        public void TestEnums()
        {
            WithResponse("enum?input=One", r => Assert.AreEqual("One", r.GetContent()));
        }

        [TestMethod]
        public void TestNullableSet()
        {
            WithResponse("nullable?input=1", r => Assert.AreEqual("1", r.GetContent()));
        }

        [TestMethod]
        public void TestNullableNotSet()
        {
            WithResponse("nullable", r => Assert.AreEqual(HttpStatusCode.NoContent, r.StatusCode));
        }

        [TestMethod]
        public void TestGuid()
        {
            var id = Guid.NewGuid().ToString();

            WithResponse($"guid?id={id}", r => Assert.AreEqual(id, r.GetContent()));
        }

        [TestMethod]
        public void TestParam()
        {
            WithResponse("param/42", r => Assert.AreEqual("42", r.GetContent()));
        }

        [TestMethod]
        public void TestConversionFailure()
        {
            WithResponse("param/abc", r => Assert.AreEqual(HttpStatusCode.BadRequest, r.StatusCode));
        }

        [TestMethod]
        public void TestRegex()
        {
            WithResponse("regex/42", r => Assert.AreEqual("42", r.GetContent()));
        }

        [TestMethod]
        public void TestEntityWithNulls()
        {
            var entity = "{\"id\":42}";
            WithResponse("entity", "POST", entity, null, null, r => Assert.AreEqual(entity, r.GetContent()));
        }

        [TestMethod]
        public void TestEntityWithNoNulls()
        {
            var entity = "{\"id\":42,\"nullable\":123.456}";
            WithResponse("entity", "POST", entity, null, null, r => Assert.AreEqual(entity, r.GetContent()));
        }

        [TestMethod]
        public void TestNotSupportedUpload()
        {
            WithResponse("entity", "POST", "123", "bla/blubb", null, r => Assert.AreEqual(HttpStatusCode.UnsupportedMediaType, r.StatusCode));
        }

        [TestMethod]
        public void TestUnsupportedDownloadEnforcesDefault()
        {
            var entity = "{\"id\":42,\"nullable\":123.456}";
            WithResponse("entity", "POST", entity, null, "bla/blubb", r => Assert.AreEqual(entity, r.GetContent()));
        }

        [TestMethod]
        public void TestWrongMethod()
        {
            WithResponse("entity", "PUT", "123", null, null, r => Assert.AreEqual(HttpStatusCode.MethodNotAllowed, r.StatusCode));
        }

        [TestMethod]
        public void TestNoMethod()
        {
            WithResponse("idonotexist", r => Assert.AreEqual(HttpStatusCode.NotFound, r.StatusCode));
        }

        [TestMethod]
        public void TestStream()
        {
            WithResponse("stream", "PUT", "123456", null, null, r => Assert.AreEqual("6", r.GetContent()));
        }

        [TestMethod]
        public void TestRequestResponse()
        {
            WithResponse("requestResponse", r => Assert.AreEqual("Hello World", r.GetContent()));
        }

        [TestMethod]
        public void TestRouting()
        {
            WithResponse("request", r => Assert.AreEqual("yes", r.GetContent()));
        }

        [TestMethod]
        public void TestEntityAsXML()
        {
            var entity = "<TestEntity><ID>1</ID><Nullable>1234.56</Nullable></TestEntity>";

            WithResponse("entity", "POST", entity, "text/xml", "text/xml", r =>
            {
                var result = new XmlSerializer(typeof(TestEntity)).Deserialize(r.GetResponseStream()) as TestEntity;

                Assert.IsNotNull(result);

                Assert.AreEqual(1, result!.ID);
                Assert.AreEqual(1234.56, result!.Nullable);
            });
        }

        [TestMethod]
        public void TestException()
        {
            WithResponse("exception", r => Assert.AreEqual(HttpStatusCode.AlreadyReported, r.StatusCode));
        }

        [TestMethod]
        public void TestDuplicate()
        {
            WithResponse("duplicate", r => Assert.AreEqual(HttpStatusCode.BadRequest, r.StatusCode));
        }

        [TestMethod]
        public void TestWithInstance()
        {
            var layout = Layout.Create().AddService("t", new TestResource());

            using var runner = TestRunner.Run(layout);

            using var response = runner.GetResponse("/t");

            Assert.AreEqual(HttpStatusCode.NoContent, response.StatusCode);
        }

        #endregion

        #region Helpers

        private void WithResponse(string uri, Action<HttpWebResponse> logic) => WithResponse(uri, "GET", null, null, null, logic);

        private void WithResponse(string uri, string method, string? body, string? contentType, string? accept, Action<HttpWebResponse> logic)
        {
            using var service = GetService();

            var request = service.GetRequest($"/t/{uri}");

            request.Method = method;

            if (accept is not null)
            {
                request.Accept = accept;
            }

            if (body is not null)
            {
                if (contentType is not null)
                {
                    request.ContentType = contentType;
                }

                using (var input = request.GetRequestStream())
                {
                    input.Write(Encoding.UTF8.GetBytes(body));
                }
            }

            using var response = request.GetSafeResponse();

            logic(response);
        }

        private TestRunner GetService()
        {
            return TestRunner.Run(Layout.Create().AddService<TestResource>("t"));
        }

        #endregion

    }


}
