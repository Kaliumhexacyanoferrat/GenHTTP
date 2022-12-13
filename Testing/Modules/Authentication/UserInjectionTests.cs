using System.Net;
using System.Threading.Tasks;

using GenHTTP.Modules.Authentication;
using GenHTTP.Modules.Authentication.Basic;
using GenHTTP.Modules.Functional;
using GenHTTP.Modules.Reflection;
using GenHTTP.Modules.Reflection.Injectors;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GenHTTP.Testing.Acceptance.Modules.Authentication
{

    [TestClass]
    public class UserInjectionTests
    {

        #region Tests

        [TestMethod]
        public async Task TestUserInjected()
        {
            using var runner = GetRunner();

            using var client = TestRunner.GetClient(creds: new NetworkCredential("abc", "def"));

            using var response = await runner.GetResponse(client: client);

            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            Assert.AreEqual("abc", await response.GetContent());
        }

        [TestMethod]
        public async Task TestNoUser()
        {
            using var runner = GetRunner();

            using var response = await runner.GetResponse();

            Assert.AreEqual(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        #endregion

        #region Helpers

        private static TestRunner GetRunner()
        {
            var auth = BasicAuthentication.Create()
                                          .Add("abc", "def");

            var injection = Injection.Empty()
                                     .Add(new UserInjector<BasicAuthenticationUser>());

            var content = Inline.Create()
                                .Get((BasicAuthenticationUser user) => user.DisplayName)
                                .Injectors(injection)
                                .Authentication(auth);

            return TestRunner.Run(content);
        }

        #endregion

    }

}
