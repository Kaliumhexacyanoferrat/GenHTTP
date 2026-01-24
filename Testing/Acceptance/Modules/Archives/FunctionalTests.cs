using GenHTTP.Api.Content.IO;
using GenHTTP.Api.Protocol;
using GenHTTP.Api.Routing;

using GenHTTP.Modules.Archives;
using GenHTTP.Modules.IO;
using GenHTTP.Testing.Acceptance.Utilities;

namespace GenHTTP.Testing.Acceptance.Modules.Archives;

[TestClass]
public class FunctionalTests
{

    #region Tests

    [TestMethod]
    public async Task TestFormats()
    {
        foreach (var format in new[] { "zip", "tar.gz", "7z" })
        {
            var file = Resource.FromAssembly($"Archive.{format}");

            var tree = ArchiveTree.From(file).Build();

            Assert.IsNotNull(tree.Modified);

            var (foundNode, foundFile) = await tree.Find(new RoutingTarget(WebPath.FromString("/SubDir/SubDir/SubFile.txt")));

            Assert.IsNotNull(foundNode);
            Assert.IsNotNull(foundFile);

            Assert.AreEqual("SubFile.txt", foundFile.Name);

            Assert.AreEqual(ContentType.TextPlain, foundFile.ContentType?.KnownType);
            Assert.IsNotNull(foundFile.Modified);

            Assert.AreEqual("3", await GetContentAsync(foundFile));
        }
    }

    [TestMethod]
    public async Task TestNonSeekableResource()
    {
        var source = new NonSeekableResource(Resource.FromAssembly("Archive.zip").Build());

        var tree = ArchiveTree.From(source).Build();

        var (_, foundFile) = await tree.Find(new RoutingTarget(WebPath.FromString("/SubDir/SubDir/SubFile.txt")));

        Assert.AreEqual("3", await GetContentAsync(foundFile!));
    }

    [TestMethod]
    public async Task TestNavigation()
    {
        var source = Resource.FromAssembly("Archive.zip");

        var tree = ArchiveTree.From(source).Build();

        Assert.HasCount(1, await tree.GetNodes());
        Assert.HasCount(1, await tree.GetResources());

        Assert.IsNotNull(await tree.TryGetResourceAsync("RootFile.txt"));
        Assert.IsNotNull(await tree.TryGetNodeAsync("SubDir"));

        Assert.IsNotNull(tree.Modified);
    }

    [TestMethod]
    public async Task TestResourceImplementation()
    {
        var source = Resource.FromAssembly("Archive.tar.gz").Build();

        var tree = ArchiveTree.From(source).Build();

        var file = (await tree.TryGetResourceAsync("RootFile.txt"))!;

        await file.CalculateChecksumAsync();

        Assert.AreEqual((ulong)1, file.Length);
    }

    #endregion

    #region Helpers

    private static async ValueTask<string> GetContentAsync(IResource resource)
    {
        await using var content = await resource.GetContentAsync();

        using var reader = new StreamReader(content);

        return await reader.ReadToEndAsync();
    }

    #endregion

}
