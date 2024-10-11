using Microsoft.VisualStudio.TestTools.UnitTesting;

using GenHTTP.Modules.IO;
using GenHTTP.Modules.Layouting;

namespace GenHTTP.Testing.Acceptance.Engine;

[TestClass]
public sealed class EncodingTests
{

    /// <summary>
    /// As a developer, I want UTF-8 to be my default encoding.
    /// </summary>
    [TestMethod]
    public async Task TestUtf8DefaultEncoding()
    {
            var layout = Layout.Create().Add("utf8", Content.From(Resource.FromString("From GenHTTP with ❤")));

            using var runner = TestHost.Run(layout);

            using var response = await runner.GetResponseAsync("/utf8");

            Assert.AreEqual("From GenHTTP with ❤", await response.GetContentAsync());
        }

}
