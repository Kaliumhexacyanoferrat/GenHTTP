﻿using GenHTTP.Api.Content.IO;
using GenHTTP.Api.Routing;

using GenHTTP.Modules.IO;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GenHTTP.Testing.Acceptance.Modules.IO
{

    [TestClass]
    public sealed class VirtualTreeTests
    {

        [TestMethod]
        public void TestNestedTree()
        {
            var tree = ResourceTree.FromAssembly("Resources");

            var virt = VirtualTree.Create()
                                  .Add("r", tree)
                                  .Build();

            var (node, file) = virt.Find(GetTarget("/r/File.txt"));

            Assert.IsNotNull(node);
            Assert.IsNotNull(file);

            Assert.IsTrue((node as IResourceNode)?.Parent == virt);

            Assert.IsNotNull(virt.Modified);
        }

        [TestMethod]
        public void TestResource()
        {
            var virt = VirtualTree.Create()
                                  .Add("res.txt", Resource.FromString("Blubb"))
                                  .Build();

            var (node, file) = virt.Find(GetTarget("/res.txt"));

            Assert.IsNotNull(node);
            Assert.IsNotNull(file);
        }

        private static RoutingTarget GetTarget(string path) => new(new PathBuilder(path).Build());

    }

}
