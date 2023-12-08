using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

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
        public async Task TestEnumerableViewModel()
        {
            var vm = new List<int>() { 4711 };

            var page = ModScriban.Page<ViewModel<List<int>>>(Resource.FromString("Hello World!"), (r, h) => new(new ViewModel<List<int>>(r, h, vm)));

            using var runner = TestHost.Run(page);

            using var r1 = await runner.GetResponseAsync();

            vm.Add(0815);

            using var r2 = await runner.GetResponseAsync();

            Assert.AreNotEqual(r1.GetHeader("ETag"), r2.GetHeader("ETag"));
        }

        [TestMethod]
        public async Task TestNonEnumerableViewModel()
        {
            var vm = new StringBuilder();

            var page = ModScriban.Page<ViewModel<StringBuilder>>(Resource.FromString("Hello World!"), (r, h) => new(new ViewModel<StringBuilder>(r, h, vm)));

            using var runner = TestHost.Run(page);

            using var r1 = await runner.GetResponseAsync();

            using var r2 = await runner.GetResponseAsync();

            Assert.AreEqual(r1.GetHeader("ETag"), r2.GetHeader("ETag"));
        }

    }

}
