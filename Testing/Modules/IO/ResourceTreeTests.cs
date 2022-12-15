using System.Linq;
using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using GenHTTP.Modules.IO;

namespace GenHTTP.Testing.Acceptance.Modules.IO
{

    [TestClass]
    public sealed class ResourceTreeTests
    {

        [TestMethod]
        public async Task TestAssembly()
        {
            var tree = ResourceTree.FromAssembly("Resources").Build();

            Assert.IsNotNull(await tree.TryGetNodeAsync("Subdirectory"));
            
            Assert.IsNotNull(await tree.TryGetResourceAsync("File.txt"));

            Assert.AreEqual(1, await tree.GetNodes().CountAsync());

            Assert.AreEqual(5, await tree.GetResources().CountAsync());
        }

    }

}
