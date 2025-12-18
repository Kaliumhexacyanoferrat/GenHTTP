using System.Net;
using GenHTTP.Modules.Conversion;
using GenHTTP.Modules.Layouting;
using GenHTTP.Modules.Reflection;
using GenHTTP.Modules.Webservices;

namespace GenHTTP.Testing.Acceptance.Modules.Webservices;

[TestClass]
public sealed class ExtensionTests
{

    #region Tests

    [TestMethod]
    [MultiEngineFrameworkTest]
    public async Task TestConfiguration(TestEngine engine, ExecutionMode mode)
    {
        var injectors = Injection.Default();

        var formats = Serialization.Default();

        var app = Layout.Create()
                        .AddService<TestService>("by-type", injectors, formats, mode: mode)
                        .AddService("by-instance", new TestService(), injectors, formats, mode: mode);

        await using var host = await TestHost.RunAsync(app, engine: engine);

        using var r1 = await host.GetResponseAsync("/by-type");

        await r1.AssertStatusAsync(HttpStatusCode.OK);

        using var r2 = await host.GetResponseAsync("/by-instance");

        await r2.AssertStatusAsync(HttpStatusCode.OK);
    }

    #endregion

    #region Supporting data structures

    public class TestService
    {

        [ResourceMethod]
        public int DoWork() => 42;
    }

    #endregion

}
