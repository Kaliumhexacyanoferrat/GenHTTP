using Xunit;

using GenHTTP.Modules.IO;
using GenHTTP.Modules.Layouting;

namespace GenHTTP.Testing.Acceptance.Engine
{

    public class EncodingTests
    {

        /// <summary>
        /// As a developer, I want UTF-8 to be my default encoding.
        /// </summary>
        [Fact]
        public void TestUtf8DefaultEncoding()
        {
            var layout = Layout.Create().Add("utf8", Content.FromString("From GenHTTP with ❤"));

            using (var runner = TestRunner.Run(layout))
            {
                using var response = runner.GetResponse("/utf8");
                Assert.Equal("From GenHTTP with ❤", response.GetContent());
            }
        }

    }

}
