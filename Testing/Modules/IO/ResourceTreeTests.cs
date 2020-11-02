using GenHTTP.Modules.IO;
using System.Linq;
using Xunit;

namespace GenHTTP.Testing.Acceptance.Modules.IO
{

    public class ResourceTreeTests
    {

        [Fact]
        public void TestAssembly()
        {
            var tree = ResourceTree.FromAssembly("Resources").Build();

            Assert.True(tree.TryGetNode("Subdirectory", out var _));
            
            Assert.True(tree.TryGetResource("File.txt", out var _));

            Assert.Single(tree.GetNodes());

            Assert.Equal(4, tree.GetResources().Count());
        }

    }

}
