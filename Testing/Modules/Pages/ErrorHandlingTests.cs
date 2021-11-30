using System.Net;
using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using GenHTTP.Modules.IO;
using GenHTTP.Modules.Scriban;

namespace GenHTTP.Testing.Acceptance.Modules.Pages
{

    [TestClass]
    public class ErrorHandlingTests
    {

        [TestMethod]
        public async Task TestErrorPage()
        {
            var page = ModScriban.Page(Resource.FromString("{{i.will.fail}}"));

            using var runner = TestRunner.Run(page);

            using var response = await runner.GetResponse();

            Assert.AreEqual(HttpStatusCode.InternalServerError, response.StatusCode);
        }

    }

}
