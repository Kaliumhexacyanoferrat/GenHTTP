using GenHTTP.Api.Protocol;
using GenHTTP.Modules.Functional;

namespace GenHTTP.Testing.Acceptance.Engine.Internal.Security;

[TestClass]
public class HeaderInjectionTests : WireTest
{

    [TestMethod]
    public async Task TryViaBackslash()
    {
        var request = new []
        {
            "GET / HTTP/1.1",
            "Host: bla\\r\\nInjected:Yes"
        };

        await TestInjection(request);
    }

    [TestMethod]
    public async Task TryViaEncoding()
    {
        var request = new []
        {
            "GET / HTTP/1.1",
            "Host: bla%0D%0AInjected:Yes"
        };

        await TestInjection(request);
    }

    private async Task TestInjection(string[] request)
    {
        var app = Inline.Create()
                        .Get((IRequest r) => r.Headers.ContainsKey("Injected") ? "Yes" : "No");

        await TestAsync(request, "No");
    }

}
