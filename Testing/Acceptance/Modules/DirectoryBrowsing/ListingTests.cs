using System.Net;
using GenHTTP.Modules.DirectoryBrowsing;
using GenHTTP.Modules.IO;
using GenHTTP.Testing.Acceptance.Utilities;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GenHTTP.Testing.Acceptance.Modules.DirectoryBrowsing;

[TestClass]
public sealed class ListingTests
{

    /// <summary>
    /// As an user of a web application, I can view the folders and files available
    /// on root level of a listed directory.
    /// </summary>
    [TestMethod]
    [MultiEngineTest]
    public async Task TestGetMainListing(TestEngine engine)
    {
        await using var runner = await GetEnvironmentAsync(engine);

        using var response = await runner.GetResponseAsync("/");

        var content = await response.GetContentAsync();

        AssertX.Contains("Subdirectory", content);
        AssertX.Contains("With%20Spaces", content);

        AssertX.Contains("my.txt", content);

        AssertX.DoesNotContain("..", content);
    }

    /// <summary>
    /// As an user of a web application, I can view the folders and files available
    /// within a subdirectory of a listed directory.
    /// </summary>
    [TestMethod]
    [MultiEngineTest]
    public async Task TestGetSubdirectory(TestEngine engine)
    {
        await using var runner = await GetEnvironmentAsync(engine);

        using var response = await runner.GetResponseAsync("/Subdirectory/");

        var content = await response.GetContentAsync();

        AssertX.Contains("..", content);
    }

    /// <summary>
    /// As an user of a web application, I can download the files listed by the
    /// directory listing feature.
    /// </summary>
    [TestMethod]
    [MultiEngineTest]
    public async Task TestDownload(TestEngine engine)
    {
        await using var runner = await GetEnvironmentAsync(engine);

        using var response = await runner.GetResponseAsync("/my.txt");

        Assert.AreEqual("Hello World!", await response.GetContentAsync());
    }

    [TestMethod]
    [MultiEngineTest]
    public async Task TestNonExistingFolder(TestEngine engine)
    {
        await using var runner = await GetEnvironmentAsync(engine);

        using var response = await runner.GetResponseAsync("/idonotexist/");

        await response.AssertStatusAsync(HttpStatusCode.NotFound);
    }

    [TestMethod]
    [MultiEngineTest]
    public async Task TestSameListingSameChecksum(TestEngine engine)
    {
        await using var runner = await GetEnvironmentAsync(engine);

        using var resp1 = await runner.GetResponseAsync();
        using var resp2 = await runner.GetResponseAsync();

        Assert.IsNotNull(resp1.GetETag());

        Assert.AreEqual(resp1.GetETag(), resp2.GetETag());
    }

    private static async Task<TestHost> GetEnvironmentAsync(TestEngine engine)
    {
        var tempFolder = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());

        Directory.CreateDirectory(tempFolder);

        Directory.CreateDirectory(Path.Combine(tempFolder, "Subdirectory"));

        Directory.CreateDirectory(Path.Combine(tempFolder, "With Spaces"));

        FileUtil.WriteText(Path.Combine(tempFolder, "my.txt"), "Hello World!");

        var listing = Listing.From(ResourceTree.FromDirectory(tempFolder));

        return await TestHost.RunAsync(listing, engine: engine);
    }
}
