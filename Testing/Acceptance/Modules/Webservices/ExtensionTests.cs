using System.Net;
using GenHTTP.Modules.Conversion;
using GenHTTP.Modules.Layouting;
using GenHTTP.Modules.Reflection;
using GenHTTP.Modules.Webservices;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GenHTTP.Testing.Acceptance.Modules.Webservices;

[TestClass]
public sealed class ExtensionTests
{

    #region Supporting data structures

    public class TestService
    {

        [ResourceMethod]
        public int DoWork() => 42;

    }

    #endregion

    #region Tests

    [TestMethod]
    public async Task TestConfiguration()
    {
            var injectors = Injection.Default();

            var formats = Serialization.Default();

            var app = Layout.Create()
                            .AddService<TestService>("by-type", injectors, formats)
                            .AddService("by-instance", new TestService(), injectors, formats);

            using var host = TestHost.Run(app);

            using var r1 = await host.GetResponseAsync("/by-type");

            await r1.AssertStatusAsync(HttpStatusCode.OK);

            using var r2 = await host.GetResponseAsync("/by-instance");

            await r2.AssertStatusAsync(HttpStatusCode.OK);
        }

    #endregion

}
