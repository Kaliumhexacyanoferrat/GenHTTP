using System.Reflection;

using GenHTTP.Modules.IO;

namespace GenHTTP.Testing.Acceptance.Modules.IO;

[TestClass]
public sealed class ResourceTreeTests
{

    [TestMethod]
    public async Task TestAssemblyByName()
    {
        var tree = ResourceTree.FromAssembly("Resources").Build();

        Assert.IsNotNull(await tree.TryGetNodeAsync("Subdirectory"));

        Assert.IsNotNull(await tree.TryGetResourceAsync("File.txt"));

        Assert.HasCount(1, await tree.GetNodes());

        Assert.HasCount(6, await tree.GetResources());
    }

    [TestMethod]
    public async Task TestByAssembly()
    {
        var tree = ResourceTree.FromAssembly(Assembly.GetExecutingAssembly()).Build();

        Assert.HasCount(1, await tree.GetNodes());
    }

}
