using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

using GenHTTP.Modules.Functional;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GenHTTP.Testing.Acceptance.Modules.Conversion
{

    [TestClass]
    public class ErrorHandlingTests
    {

        #region Supporting data structures

        record class MyEntity(string Data);

        #endregion

        #region Tests

        [TestMethod]
        public async Task UndeserializableBodyReturnsWithBadRequest()
        {
            var inline = Inline.Create()
                               .Post("/t", (MyEntity entity) => entity.Data);

            using var runner = TestRunner.Run(inline);

            using var request = runner.GetRequest("/t");

            request.Method = HttpMethod.Post;

            request.Content = new StringContent("I cannot be deserialized", null, "application/json");
            request.Content.Headers.ContentType = new("application/json");

            using var response = await runner.GetResponse(request);

            Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
        }

        #endregion

    }

}
