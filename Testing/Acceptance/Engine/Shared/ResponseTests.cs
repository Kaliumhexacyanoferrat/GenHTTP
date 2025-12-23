using GenHTTP.Engine.Shared.Types;

namespace GenHTTP.Testing.Acceptance.Engine.Shared;

[TestClass]
public class ResponseTests
{

    [TestMethod]
    public void TestCookies()
    {
        using var response = new Response();

        response.SetCookie(new("Key", "Value"));

        Assert.HasCount(1, response.Cookies);

        Assert.AreEqual("Value", response.Cookies["key"].Value);
    }

    [TestMethod]
    public void TestHeaders()
    {
        using var response = new Response();

        response["Key"] = "Value";

        Assert.HasCount(1, response.Headers);
        Assert.AreEqual("Value", response.Headers["key"]);
    }

}
