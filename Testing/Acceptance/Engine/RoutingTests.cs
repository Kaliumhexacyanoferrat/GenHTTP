using GenHTTP.Api.Routing;

namespace GenHTTP.Testing.Acceptance.Engine;

[TestClass]
public class RoutingTests
{

    [TestMethod]
    public void TestComparers()
    {
        var path = new WebPathPart("%C3%A4%2F");

        Assert.IsTrue(path == "ä/");
        Assert.IsTrue(path == "%C3%A4%2F");

        Assert.IsTrue(path != "ö/");
    }

    [TestMethod]
    public void TestEquality()
    {
        var one = new WebPathPart("%C3%A4%2F");
        var two = new WebPathPart("%C3%A4%2F");

        Assert.IsTrue(one.Equals(two));
        Assert.IsTrue(two.Equals(one));

        var three = new WebPathPart("ä/");
        
        Assert.IsFalse(one.Equals(three));
        Assert.IsFalse(one.Equals(new List<int>()));
        Assert.IsFalse(one.Equals(null));
    }

}
