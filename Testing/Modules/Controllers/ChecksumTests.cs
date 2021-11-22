using System.Collections.Generic;
using System.Text;
using GenHTTP.Api.Content.Templating;
using GenHTTP.Modules.IO;
using GenHTTP.Modules.Scriban;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GenHTTP.Testing.Acceptance.Modules.Controllers
{

    [TestClass]
    public sealed class ChecksumTests
    {

        [TestMethod]
        public void TestEnumerableViewModel()
        {
            var vm = new List<int>() { 4711 };

            var page = ModScriban.Page<ViewModel<List<int>>>(Resource.FromString("Hello World!"), (r, h) => new(new ViewModel<List<int>>(r, h, vm)));

            using var runner = TestRunner.Run(page);

            using var r1 = runner.GetResponse();

            vm.Add(0815);

            using var r2 = runner.GetResponse();

            Assert.AreNotEqual(r1.GetResponseHeader("ETag"), r2.GetResponseHeader("ETag"));
        }

        [TestMethod]
        public void TestNonEnumerableViewModel()
        {
            var vm = new StringBuilder();

            var page = ModScriban.Page<ViewModel<StringBuilder>>(Resource.FromString("Hello World!"), (r, h) => new(new ViewModel<StringBuilder>(r, h, vm)));

            using var runner = TestRunner.Run(page);

            using var r1 = runner.GetResponse();

            using var r2 = runner.GetResponse();

            Assert.AreEqual(r1.GetResponseHeader("ETag"), r2.GetResponseHeader("ETag"));
        }

    }

}
