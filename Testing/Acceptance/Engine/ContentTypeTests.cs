using GenHTTP.Api.Protocol;

namespace GenHTTP.Testing.Acceptance.Engine;

[TestClass]
public sealed class ContentTypeTests
{

    [TestMethod]
    public void MapContentTypeTests()
    {
        foreach (ContentType contentType in Enum.GetValues(typeof(ContentType)))
        {
            var mapped = new FlexibleContentType(contentType);

            Assert.AreEqual(mapped.KnownType, contentType);
        }
    }

    [TestMethod]
    public void ConcurrentContentTypeAccessTest()
    {
        Parallel.For(0, 10, _ =>
        {
            FlexibleContentType.Parse("application/json");
        });
    }

    [TestMethod]
    public void TestOperators()
    {
        var simple = ContentType.TextCss;

        var complex = FlexibleContentType.Parse("text/css;");

        Assert.IsTrue(complex == simple);
        Assert.IsTrue(complex != ContentType.TextPlain);

        Assert.IsTrue(complex == "text/css");
        Assert.IsTrue(complex != "text/plain");

        Assert.AreEqual(complex, new FlexibleContentType("text/css", "utf-8"));
    }

}
