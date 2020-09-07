using System;
using System.IO;
using System.Net;
using System.Text;
using System.Xml.Serialization;

using Xunit;

using GenHTTP.Api.Content;
using GenHTTP.Api.Protocol;

using GenHTTP.Modules.IO;
using GenHTTP.Modules.Webservices;
using GenHTTP.Modules.Basics;
using GenHTTP.Modules.Layouting;

namespace GenHTTP.Testing.Acceptance.Modules.Webservices
{

    public class WebserviceTests
    {

        #region Supporting structures

        public class TestEntity
        {

            public int ID { get; set; }

            public double? Nullable { get; set; }

        }

        public enum TestEnum
        {
            One,
            Two
        }

        public class TestResource
        {

            [ResourceMethod("nothing")]
            public void DoNothing() { }

            [ResourceMethod("primitive")]
            public int Primitive(int input) => input;

            [ResourceMethod(RequestMethod.POST, "entity")]
            public TestEntity Entity(TestEntity entity) => entity;

            [ResourceMethod(RequestMethod.PUT, "stream")]
            public Stream Stream(Stream input) => new MemoryStream(Encoding.UTF8.GetBytes(input.Length.ToString()));

            [ResourceMethod("requestResponse")]
            public IResponseBuilder RequestResponse(IRequest request)
            {
                return request.Respond()
                              .Content("Hello World")
                              .Type(ContentType.TextPlain);
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

        [Fact]
        public void TestEmpty()
        {
            WithResponse("", r => Assert.Equal(HttpStatusCode.NoContent, r.StatusCode));
        }

        [Fact]
        public void TestVoidReturn()
        {
            WithResponse("nothing", r => Assert.Equal(HttpStatusCode.NoContent, r.StatusCode));
        }

        [Fact]
        public void TestPrimitives()
        {
            WithResponse("primitive?input=42", r => Assert.Equal("42", r.GetContent()));
        }

        [Fact]
        public void TestEnums()
        {
            WithResponse("enum?input=One", r => Assert.Equal("One", r.GetContent()));
        }

        [Fact]
        public void TestNullableSet()
        {
            WithResponse("nullable?input=1", r => Assert.Equal("1", r.GetContent()));
        }

        [Fact]
        public void TestNullableNotSet()
        {
            WithResponse("nullable", r => Assert.Equal(HttpStatusCode.NoContent, r.StatusCode));
        }

        [Fact]
        public void TestParam()
        {
            WithResponse("param/42", r => Assert.Equal("42", r.GetContent()));
        }

        [Fact]
        public void TestConversionFailure()
        {
            WithResponse("param/abc", r => Assert.Equal(HttpStatusCode.BadRequest, r.StatusCode));
        }

        [Fact]
        public void TestRegex()
        {
            WithResponse("regex/42", r => Assert.Equal("42", r.GetContent()));
        }

        [Fact]
        public void TestEntityWithNulls()
        {
            var entity = "{\"id\":42}";
            WithResponse("entity", "POST", entity, null, null, r => Assert.Equal(entity, r.GetContent()));
        }

        [Fact]
        public void TestEntityWithNoNulls()
        {
            var entity = "{\"id\":42,\"nullable\":123.456}";
            WithResponse("entity", "POST", entity, null, null, r => Assert.Equal(entity, r.GetContent()));
        }

        [Fact]
        public void TestNotSupportedUpload()
        {
            WithResponse("entity", "POST", "123", "bla/blubb", null, r => Assert.Equal(HttpStatusCode.UnsupportedMediaType, r.StatusCode));
        }

        [Fact]
        public void TestUnsupportedDownloadEnforcesDefault()
        {
            var entity = "{\"id\":42,\"nullable\":123.456}";
            WithResponse("entity", "POST", entity, null, "bla/blubb", r => Assert.Equal(entity, r.GetContent()));
        }

        [Fact]
        public void TestWrongMethod()
        {
            WithResponse("entity", "PUT", "123", null, null, r => Assert.Equal(HttpStatusCode.MethodNotAllowed, r.StatusCode));
        }

        [Fact]
        public void TestNoMethod()
        {
            WithResponse("idonotexist", r => Assert.Equal(HttpStatusCode.NotFound, r.StatusCode));
        }

        [Fact]
        public void TestStream()
        {
            WithResponse("stream", "PUT", "123456", null, null, r => Assert.Equal("6", r.GetContent()));
        }

        [Fact]
        public void TestRequestResponse()
        {
            WithResponse("requestResponse", r => Assert.Equal("Hello World", r.GetContent()));
        }

        [Fact]
        public void TestRouting()
        {
            WithResponse("request", r => Assert.Equal("yes", r.GetContent()));
        }

        [Fact]
        public void TestEntityAsXML()
        {
            var entity = "<TestEntity><ID>1</ID><Nullable>1234.56</Nullable></TestEntity>";

            WithResponse("entity", "POST", entity, "text/xml", "text/xml", r =>
            {
                var result = (TestEntity)new XmlSerializer(typeof(TestEntity)).Deserialize(r.GetResponseStream());

                Assert.Equal(1, result.ID);
                Assert.Equal(1234.56, result.Nullable);
            });
        }

        [Fact]
        public void TestException()
        {
            WithResponse("exception", r => Assert.Equal(HttpStatusCode.AlreadyReported, r.StatusCode));
        }

        [Fact]
        public void TestDuplicate()
        {
            WithResponse("duplicate", r => Assert.Equal(HttpStatusCode.BadRequest, r.StatusCode));
        }

        [Fact]
        public void TestWithInstance()
        {
            var layout = Layout.Create().AddService("t", new TestResource());

            using var runner = TestRunner.Run(layout);

            using var response = runner.GetResponse("/t");

            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        }

        #endregion

        #region Helpers

        private void WithResponse(string uri, Action<HttpWebResponse> logic) => WithResponse(uri, "GET", null, null, null, logic);

        private void WithResponse(string uri, string method, string? body, string? contentType, string? accept, Action<HttpWebResponse> logic)
        {
            using var service = GetService();

            var request = service.GetRequest($"/t/{uri}");

            request.Method = method;

            if (accept != null)
            {
                request.Accept = accept;
            }

            if (body != null)
            {
                if (contentType != null)
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
