using GenHTTP.Modules.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading.Tasks;

namespace GenHTTP.Testing.Acceptance.Engine
{

    [TestClass]
    public class ChecksumTests
    {

        [TestMethod]
        public async Task TestSameErrorSameChecksum()
        {
            using var runner = TestRunner.Run();

            using var resp1 = await runner.GetResponse();
            using var resp2 = await runner.GetResponse();

            Assert.IsNotNull(resp1.GetETag());

            Assert.AreEqual(resp1.GetETag(), resp2.GetETag());
        }

        [TestMethod]
        public async Task TestSameContentSameChecksum()
        {
            using var runner = TestRunner.Run(Content.From(Resource.FromString("Hello World!")));

            using var resp1 = await runner.GetResponse();
            using var resp2 = await runner.GetResponse();

            Assert.IsNotNull(resp1.GetETag());

            Assert.AreEqual(resp1.GetETag(), resp2.GetETag());
        }

    }

}
