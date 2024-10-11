using GenHTTP.Modules.IO;
using GenHTTP.Modules.Layouting;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GenHTTP.Testing.Acceptance.Engine;

[TestClass]
public class ChecksumTests
{

    [TestMethod]
    public async Task TestSameErrorSameChecksum()
    {
            using var runner = TestHost.Run(Layout.Create());

            using var resp1 = await runner.GetResponseAsync();
            using var resp2 = await runner.GetResponseAsync();

            Assert.IsNotNull(resp1.GetETag());

            Assert.AreEqual(resp1.GetETag(), resp2.GetETag());
        }

    [TestMethod]
    public async Task TestSameContentSameChecksum()
    {
            using var runner = TestHost.Run(Content.From(Resource.FromString("Hello World!")));

            using var resp1 = await runner.GetResponseAsync();
            using var resp2 = await runner.GetResponseAsync();

            Assert.IsNotNull(resp1.GetETag());

            Assert.AreEqual(resp1.GetETag(), resp2.GetETag());
        }

}
