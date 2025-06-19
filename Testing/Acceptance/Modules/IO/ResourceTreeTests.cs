using System.Reflection;

using GenHTTP.Modules.IO;

using Microsoft.VisualStudio.TestTools.UnitTesting;

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

        Assert.AreEqual(1, (await tree.GetNodes()).Count);

        Assert.AreEqual(5, (await tree.GetResources()).Count);
    }

    [TestMethod]
    public async Task TestByAssembly()
    {
        var tree = ResourceTree.FromAssembly(Assembly.GetExecutingAssembly()).Build();

        Assert.AreEqual(1, (await tree.GetNodes()).Count);
    }

}
