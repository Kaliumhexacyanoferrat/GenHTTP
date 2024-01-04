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

            using var client = TestHost.GetClient(creds: new NetworkCredential("abc", "def"));

            using var response = await runner.GetResponseAsync(client: client);

            await response.AssertStatusAsync(HttpStatusCode.OK);
            Assert.AreEqual("abc", await response.GetContentAsync());
        }

        [TestMethod]
        public async Task TestNoUser()
        {
            using var runner = GetRunner();

            using var response = await runner.GetResponseAsync();

            await response.AssertStatusAsync(HttpStatusCode.Unauthorized);
        }

        #endregion

        #region Helpers

        private static TestHost GetRunner()
        {
            var auth = BasicAuthentication.Create()
                                          .Add("abc", "def");

            var injection = Injection.Empty()
                                     .Add(new UserInjector<BasicAuthenticationUser>());

            var content = Inline.Create()
                                .Get((BasicAuthenticationUser user) => user.DisplayName)
                                .Injectors(injection)
                                .Authentication(auth);

            return TestHost.Run(content);
        }

        #endregion

    }

}
