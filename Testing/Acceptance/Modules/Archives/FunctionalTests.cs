using GenHTTP.Api.Protocol;
using GenHTTP.Api.Routing;

using GenHTTP.Modules.Archives;
using GenHTTP.Modules.IO;

namespace GenHTTP.Testing.Acceptance.Modules.Archives;

[TestClass]
public class FunctionalTests
{

    [TestMethod]
    public async Task TestFormats()
    {
        foreach (var format in new[] { "zip", "tar", "7z" })
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

            await using var content = await foundFile.GetContentAsync();

            using var reader = new StreamReader(content);

            var textContent = await reader.ReadToEndAsync();

            Assert.AreEqual("3", textContent);
        }
    }

}
