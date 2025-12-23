using GenHTTP.Api.Protocol;

namespace GenHTTP.Testing.Acceptance.Engine;

[TestClass]
public class ResponseStatusTests
{

    [TestMethod]
    public void TestOperators()
    {
        var known = ResponseStatus.Conflict;

        var complex = new FlexibleResponseStatus(409, "Conflict");

        Assert.IsTrue(complex == known);
        Assert.IsTrue(complex != ResponseStatus.NoContent);

        Assert.IsTrue(complex == 409);
        Assert.IsTrue(complex != 204);

        Assert.AreEqual(complex, new FlexibleResponseStatus(ResponseStatus.Conflict));
    }

}
