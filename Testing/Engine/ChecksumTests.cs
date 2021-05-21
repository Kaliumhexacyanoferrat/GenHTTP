using GenHTTP.Modules.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GenHTTP.Testing.Acceptance.Engine
{

    [TestClass]
    public class ChecksumTests
    {

        [TestMethod]
        public void TestSameErrorSameChecksum()
        {
            using var runner = TestRunner.Run();

            using var resp1 = runner.GetResponse();
            using var resp2 = runner.GetResponse();

            Assert.IsNotNull(resp1.GetETag());

            Assert.AreEqual(resp1.GetETag(), resp2.GetETag());
        }

        [TestMethod]
        public void TestSameContentSameChecksum()
        {
            using var runner = TestRunner.Run(Content.From(Resource.FromString("Hello World!")));

            using var resp1 = runner.GetResponse();
            using var resp2 = runner.GetResponse();

            Assert.IsNotNull(resp1.GetETag());

            Assert.AreEqual(resp1.GetETag(), resp2.GetETag());
        }

    }

}
