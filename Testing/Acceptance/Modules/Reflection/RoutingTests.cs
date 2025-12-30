using GenHTTP.Api.Routing;

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

        var target = new RoutingTarget(WebPath.FromString("/segment"));
        
        var match = OperationRouter.TryMatch(target, route);
        
        Assert.IsNotNull(match);
        
        Assert.AreEqual(0, match.Offset);
        Assert.IsNull(match.PathArguments);
    }
    
    [TestMethod]
    public void TestSimpleSegmentWithTrailingSlash()
    {
        var route = CreateRoute(false, new StringSegment("segment"), new ClosingSegment(false, false));

        var target = new RoutingTarget(WebPath.FromString("/segment/"));
        
        var match = OperationRouter.TryMatch(target, route);
        
        Assert.IsNotNull(match);
        
        Assert.AreEqual(1, match.Offset);
        Assert.IsNull(match.PathArguments);
    }
    
    [TestMethod]
    public void TestMissingSlashFailsRouting()
    {
        var route = CreateRoute(false, new StringSegment("segment"), new ClosingSegment(true, false));

        var target = new RoutingTarget(WebPath.FromString("/segment"));
        
        var match = OperationRouter.TryMatch(target, route);
        
        Assert.IsNull(match);
    }
    
    [TestMethod]
    public void TestNonWildcardDoesNotAllowAdditionalSegments()
    {
        var route = CreateRoute(false, new StringSegment("segment"), new ClosingSegment(false, false));

        var target = new RoutingTarget(WebPath.FromString("/segment/another"));
        
        var match = OperationRouter.TryMatch(target, route);
        
        Assert.IsNull(match);
    }
    
    [TestMethod]
    public void TestWildcardAllowsAdditionalSegments()
    {
        var route = CreateRoute(true, new StringSegment("segment"), new ClosingSegment(false, true));

        var target = new RoutingTarget(WebPath.FromString("/segment/another"));
        
        var match = OperationRouter.TryMatch(target, route);
        
        Assert.IsNotNull(match);
        
        Assert.AreEqual(1, match.Offset);
        Assert.IsNull(match.PathArguments);
    }
    
    [TestMethod]
    public void TestSimpleVariable()
    {
        var route = CreateRoute(false, new SimpleVariableSegment("var"));

        var target = new RoutingTarget(WebPath.FromString("/99/"));
        
        var match = OperationRouter.TryMatch(target, route);
        
        Assert.IsNotNull(match);
        
        Assert.AreEqual(1, match.Offset);
        
        Assert.IsNotNull(match.PathArguments);
        Assert.HasCount(1, match.PathArguments);
        
        Assert.AreEqual("99", match.PathArguments["var"]);
    }
    
    [TestMethod]
    public void TestComplexVariables()
    {
        var route = CreateRoute(false, new RegexSegment("(?<one>[0-9]{2})-(?<two>[0-9]{2})-:three"));

        var target = new RoutingTarget(WebPath.FromString("/99-88-77"));
        
        var match = OperationRouter.TryMatch(target, route);
        
        Assert.IsNotNull(match);
        
        Assert.AreEqual(1, match.Offset);
        
        Assert.IsNotNull(match.PathArguments);
        Assert.HasCount(3, match.PathArguments);
        
        Assert.AreEqual("99", match.PathArguments["one"]);
        Assert.AreEqual("88", match.PathArguments["two"]);
        Assert.AreEqual("77", match.PathArguments["three"]);
    }

    private static OperationRoute CreateRoute(bool wildcard, params IRoutingSegment[] segments) => new("name", segments, wildcard);

}
