using System.Net;

using GenHTTP.Api.Infrastructure;
using GenHTTP.Api.Protocol;

using GenHTTP.Modules.DirectoryBrowsing;
using GenHTTP.Modules.Files;
using GenHTTP.Modules.IO;
using GenHTTP.Modules.SinglePageApplications;
using GenHTTP.Modules.StaticWebsites;

namespace GenHTTP.Testing.Acceptance.Modules.IO;

/// <summary>
/// Verifies that every file serving handler rejects path traversal attempts
/// via <see cref="RequestSecurityExtensions.DenyPathTraversal" />.
/// </summary>
/// <remarks>
/// Each handler is probed with a different forged segment so that, taken
/// together, the tests exercise all branches of the detection logic (plain
/// dots, upper- and lower-case percent encoding, mixed forms and the negative
/// cases that must not be denied).
/// </remarks>
[TestClass]
public sealed class PathTraversalTests
{

    #region Denial (one case per affected handler)

    [TestMethod]
    [MultiEngineTest]
    public async Task TestAssetsTreeDeniesEncodedTraversal(ServerEngine engine)
    {
        // TreeAssetsHandler (via AbstractAssetsHandler) - percent encoded ".." ("%2e%2e")
        await using var runner = await TestHost.RunAsync(Assets.From(ResourceTree.FromDirectory(CreateRoot())), engine: engine);

        using var response = await GetForgedAsync(runner, "/sub/%2e%2e/file.txt");

        await response.AssertStatusAsync(HttpStatusCode.BadRequest);
    }

    [TestMethod]
    [MultiEngineTest]
    public async Task TestAssetsDirectoryDeniesLiteralTraversal(ServerEngine engine)
    {
        // BuiltInFileAssetHandler (via AbstractAssetsHandler) - literal ".."
        await using var runner = await TestHost.RunAsync(Assets.From(CreateRoot()), engine: engine);

        using var response = await GetForgedAsync(runner, "/sub/../file.txt");

        await response.AssertStatusAsync(HttpStatusCode.BadRequest);
    }

    [TestMethod]
    [MultiEngineTest]
    public async Task TestStaticWebsiteDeniesUppercaseEncodedTraversal(ServerEngine engine)
    {
        // StaticWebsiteHandler trailing slash branch (bypasses AbstractAssetsHandler) - upper case "%2E"
        await using var runner = await TestHost.RunAsync(StaticWebsite.From(ResourceTree.FromDirectory(CreateRoot())), engine: engine);

        using var response = await GetForgedAsync(runner, "/sub/%2E/");

        await response.AssertStatusAsync(HttpStatusCode.BadRequest);
    }

    [TestMethod]
    [MultiEngineTest]
    public async Task TestSinglePageDeniesMixedTraversal(ServerEngine engine)
    {
        // SinglePageProvider - mixed literal and encoded dot (".%2e")
        await using var runner = await TestHost.RunAsync(SinglePageApplication.From(ResourceTree.FromDirectory(CreateRoot())), engine: engine);

        using var response = await GetForgedAsync(runner, "/app/.%2e/config.json");

        await response.AssertStatusAsync(HttpStatusCode.BadRequest);
    }

    [TestMethod]
    [MultiEngineTest]
    public async Task TestDirectoryBrowsingDeniesSingleDot(ServerEngine engine)
    {
        // ListingRouter (bypasses AbstractAssetsHandler) - single "."
        await using var runner = await TestHost.RunAsync(Listing.From(ResourceTree.FromDirectory(CreateRoot())), engine: engine);

        using var response = await GetForgedAsync(runner, "/sub/./");

        await response.AssertStatusAsync(HttpStatusCode.BadRequest);
    }

    #endregion

    #region Non-traversal segments (completes detection coverage)

    [TestMethod]
    [MultiEngineTest]
    public async Task TestSuspiciousButValidSegmentsAreAllowed(ServerEngine engine)
    {
        await using var runner = await TestHost.RunAsync(Assets.From(ResourceTree.FromDirectory(CreateRoot())), engine: engine);

        // a genuine file is still served (first byte is not a dot - detection returns immediately)
        using var served = await GetForgedAsync(runner, "/file.txt");
        await served.AssertStatusAsync(HttpStatusCode.OK);

        // the following segments look suspicious but are not "." or ".." - the guard must let them
        // through (their resolved status is filesystem dependent, so we only assert they are not denied):

        // three dots - the dot count is neither 1 nor 2
        using var tripleDot = await GetForgedAsync(runner, "/.../file.txt");
        Assert.AreNotEqual(HttpStatusCode.BadRequest, tripleDot.StatusCode);

        // percent escape whose first digit is not '2' ("%41" = 'A')
        using var notTwo = await GetForgedAsync(runner, "/%41/file.txt");
        Assert.AreNotEqual(HttpStatusCode.BadRequest, notTwo.StatusCode);

        // percent escape "%2x" where x is neither 'e' nor 'E' ("%2b" = '+')
        using var notDot = await GetForgedAsync(runner, "/%2b/file.txt");
        Assert.AreNotEqual(HttpStatusCode.BadRequest, notDot.StatusCode);

        // trailing, incomplete percent escape - not enough characters to form "%2e"
        using var incomplete = await GetForgedAsync(runner, "/.%/file.txt");
        Assert.AreNotEqual(HttpStatusCode.BadRequest, incomplete.StatusCode);
    }

    #endregion

    #region Encoded separators

    [TestMethod]
    [MultiEngineTest]
    public async Task TestAssetsDirectoryDeniesEncodedSlash(ServerEngine engine)
    {
        await using var runner = await TestHost.RunAsync(Assets.From(CreateJail()), engine: engine);

        using var response = await GetForgedAsync(runner, "/..%2fsecret.txt");

        await response.AssertStatusAsync(HttpStatusCode.BadRequest);
    }

    [TestMethod]
    [MultiEngineTest]
    public async Task TestAssetsTreeDeniesFullyEncodedTraversal(ServerEngine engine)
    {
        await using var runner = await TestHost.RunAsync(Assets.From(ResourceTree.FromDirectory(CreateJail())), engine: engine);

        using var response = await GetForgedAsync(runner, "/%2e%2e%2Fsecret.txt");

        await response.AssertStatusAsync(HttpStatusCode.BadRequest);
    }

    [TestMethod]
    [MultiEngineTest]
    public async Task TestStaticWebsiteDeniesEncodedBackslash(ServerEngine engine)
    {
        await using var runner = await TestHost.RunAsync(StaticWebsite.From(ResourceTree.FromDirectory(CreateJail())), engine: engine);

        using var response = await GetForgedAsync(runner, "/..%5Csecret.txt");

        await response.AssertStatusAsync(HttpStatusCode.BadRequest);
    }

    [TestMethod]
    [MultiEngineTest]
    public async Task TestAssetsDeniesLiteralBackslash(ServerEngine engine)
    {
        await using var runner = await TestHost.RunAsync(Assets.From(CreateJail()), engine: engine);

        using var response = await GetForgedAsync(runner, "/sub\\file.txt");

        await response.AssertStatusAsync(HttpStatusCode.BadRequest);
    }

    [TestMethod]
    [MultiEngineTest]
    public async Task TestSinglePageDeniesNullByte(ServerEngine engine)
    {
        await using var runner = await TestHost.RunAsync(SinglePageApplication.From(ResourceTree.FromDirectory(CreateJail())), engine: engine);

        using var response = await GetForgedAsync(runner, "/file.txt%00.html");

        await response.AssertStatusAsync(HttpStatusCode.BadRequest);
    }

    [TestMethod]
    [MultiEngineTest]
    public async Task TestListingDoesNotFollowDriveRelativeSegment(ServerEngine engine)
    {
        await using var runner = await TestHost.RunAsync(Listing.From(ResourceTree.FromDirectory(CreateJail())), engine: engine);

        using var response = await GetForgedAsync(runner, "/C:/");

        await response.AssertStatusAsync(HttpStatusCode.NotFound);
    }

    #endregion

    #region Directory tree containment

    [TestMethod]
    public async Task TestTreeResolvesDirectChildren()
    {
        var tree = ResourceTree.FromDirectory(CreateJail()).Build();

        Assert.IsNotNull(await tree.TryGetResourceAsync(new PathSegment("file.txt")));
        Assert.IsNotNull(await tree.TryGetNodeAsync(new PathSegment("sub")));
    }

    [TestMethod]
    public async Task TestTreeDoesNotResolveOutsideOfRoot()
    {
        var root = CreateJail();

        var tree = ResourceTree.FromDirectory(root).Build();

        var secret = Path.Combine(Path.GetDirectoryName(root)!, "secret.txt");

        Assert.IsNull(await tree.TryGetResourceAsync(new PathSegment("..%2fsecret.txt")));
        Assert.IsNull(await tree.TryGetResourceAsync(new PathSegment("..%5csecret.txt")));
        Assert.IsNull(await tree.TryGetResourceAsync(new PathSegment(Uri.EscapeDataString(secret))));

        Assert.IsNull(await tree.TryGetNodeAsync(new PathSegment("..")));
        Assert.IsNull(await tree.TryGetNodeAsync(new PathSegment(".")));
        Assert.IsNull(await tree.TryGetNodeAsync(new PathSegment("sub%2f..%2f..")));
        Assert.IsNull(await tree.TryGetNodeAsync(new PathSegment("C:")));
    }

    #endregion

    #region Helpers

    private static string CreateRoot()
    {
        var root = Directory.CreateTempSubdirectory();

        File.WriteAllText(Path.Combine(root.FullName, "file.txt"), "This is a file");

        root.CreateSubdirectory("sub");

        return root.FullName;
    }

    /// <summary>
    /// Creates a served root with a secret file placed next to it, which
    /// must never be reachable through the root.
    /// </summary>
    private static string CreateJail()
    {
        var outer = Directory.CreateTempSubdirectory();

        File.WriteAllText(Path.Combine(outer.FullName, "secret.txt"), "This is a secret");

        var root = outer.CreateSubdirectory("www");

        File.WriteAllText(Path.Combine(root.FullName, "file.txt"), "This is a file");

        root.CreateSubdirectory("sub");

        return root.FullName;
    }

    /// <summary>
    /// Sends a request without client side canonicalization, so that forged
    /// segments such as ".." or "%2e%2e" actually reach the server instead of
    /// being collapsed by <see cref="Uri" />.
    /// </summary>
    private static Task<HttpResponseMessage> GetForgedAsync(TestHost runner, string rawPath)
    {
        var uri = new Uri(runner.GetUrl(rawPath), new UriCreationOptions
        {
            DangerousDisablePathAndQueryCanonicalization = true
        });

        var request = new HttpRequestMessage(HttpMethod.Get, uri);

        return runner.GetResponseAsync(request);
    }

    #endregion

}
