using GenHTTP.Modules.IO;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GenHTTP.Testing.Acceptance.Modules.IO
{

    [TestClass]
    public sealed class ResourceTreeTests
    {

        [TestMethod]
        public void TestAssembly()
        {
            var tree = ResourceTree.FromAssembly("Resources").Build();

            Assert.IsTrue(tree.TryGetNode("Subdirectory", out var _));
            
            Assert.IsTrue(tree.TryGetResource("File.txt", out var _));

            AssertX.Single(tree.GetNodes());

            Assert.AreEqual(5, tree.GetResources().Count());
        }

    }

}
