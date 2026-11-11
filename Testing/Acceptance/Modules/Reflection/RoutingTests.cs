using GenHTTP.Api.Protocol;

using GenHTTP.Engine.Shared.Types;
using GenHTTP.Modules.Reflection.Routing;
using GenHTTP.Modules.Reflection.Routing.Segments;

namespace GenHTTP.Testing.Acceptance.Modules.Reflection;

[TestClass]
public class RoutingTests
{

    [TestMethod]
    public void TestSimpleSegment()
    {
        var route = CreateRoute(false, new StringSegment("segment"), new ClosingSegment(false, false));

        var target = CreateTarget("/segment");

        var match = OperationRouter.TryMatch(target, route);

        Assert.IsNotNull(match);

        Assert.AreEqual(0, match.Value.Offset);
        Assert.IsFalse(match.Value.TryGetPathArgument(0, out _));
    }

    [TestMethod]
    public void TestSimpleSegmentWithTrailingSlash()
    {
        var route = CreateRoute(false, new StringSegment("segment"), new ClosingSegment(false, false));

        var target = CreateTarget("/segment/");

        var match = OperationRouter.TryMatch(target, route);

        Assert.IsNotNull(match);

        Assert.AreEqual(1, match.Value.Offset);
        Assert.IsFalse(match.Value.TryGetPathArgument(0, out _));
    }

    [TestMethod]
    public void TestMissingSlashFailsRouting()
    {
        var route = CreateRoute(false, new StringSegment("segment"), new ClosingSegment(true, false));

        var target = CreateTarget("/segment");

        var match = OperationRouter.TryMatch(target, route);

        Assert.IsNull(match);
    }

    [TestMethod]
    public void TestNonWildcardDoesNotAllowAdditionalSegments()
    {
        var route = CreateRoute(false, new StringSegment("segment"), new ClosingSegment(false, false));

        var target = CreateTarget("/segment/another");

        var match = OperationRouter.TryMatch(target, route);

        Assert.IsNull(match);
    }

    [TestMethod]
    public void TestWildcardAllowsAdditionalSegments()
    {
        var route = CreateRoute(true, new StringSegment("segment"), new ClosingSegment(false, true));

        var target = CreateTarget("/segment/another");

        var match = OperationRouter.TryMatch(target, route);

        Assert.IsNotNull(match);

        Assert.AreEqual(1, match.Value.Offset);
        Assert.IsFalse(match.Value.TryGetPathArgument(0, out _));
    }

    [TestMethod]
    public void TestSimpleVariable()
    {
        var route = CreateRoute(false, new SimpleVariableSegment("var"));

        var target = CreateTarget("/99/");

        var match = OperationRouter.TryMatch(target, route);

        Assert.IsNotNull(match);

        Assert.AreEqual(1, match.Value.Offset);

        CollectionAssert.AreEqual(new[] { "var" }, route.Arguments.ToArray());

        MatchArgument(match.Value, 0, "99");
    }

    [TestMethod]
    public void TestComplexVariables()
    {
        var route = CreateRoute(false, new RegexSegment("(?<one>[0-9]{2})-(?<two>[0-9]{2})-:three"));

        var target = CreateTarget("/99-88-77");

        var match = OperationRouter.TryMatch(target, route);

        Assert.IsNotNull(match);

        Assert.AreEqual(1, match.Value.Offset);

        CollectionAssert.AreEqual(new[] { "one", "two", "three" }, route.Arguments.ToArray());

        MatchArgument(match.Value, 0, "99");
        MatchArgument(match.Value, 1, "88");
        MatchArgument(match.Value, 2, "77");

        Assert.IsFalse(match.Value.TryGetPathArgument(3, out _));
    }

    private static OperationRoute CreateRoute(bool wildcard, params IRoutingSegment[] segments) => new("name", segments, wildcard);

    private static IRequestTarget CreateTarget(string path)
    {
        var target = new RequestTarget();
        target.Apply(new(path));

        return target;
    }

    private static void MatchArgument(RoutingMatch match, int index, string value)
    {
        Assert.IsTrue(match.TryGetPathArgument(index, out var argument));
        Assert.AreEqual(value, argument.ToString());
    }

}
