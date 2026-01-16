namespace GenHTTP.Testing.Acceptance.Engine.Internal.Security;

[TestClass]
public class HostHardeningTests : WireTest
{

    [TestMethod]
    public async Task TestMultipleHosts()
    {
        var request = new []
        {
            "GET / HTTP/1.1",
            "Host: bla",
            "host: blubb"
        };

        await TestAsync(request, "Multiple 'Host' headers specified");
    }

}
