namespace GenHTTP.Testing.Acceptance.Engine.Internal.Security;

[TestClass]
public class PathSegmentTests : WireTest
{

    [TestMethod]
    public async Task TestUp()
    {
        var request = new []
        {
            "GET /../ HTTP/1.1",
            "Host: host"
        };

        await TestAsync(request, "Segments '.' or '..' are now allowed in path");
    }

    [TestMethod]
    public async Task TestEncoded()
    {
        var request = new []
        {
            "GET /%2E/ HTTP/1.1",
            "Host: host"
        };

        await TestAsync(request, "Segments '.' or '..' are now allowed in path");
    }

}
