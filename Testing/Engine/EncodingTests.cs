using Xunit;

using GenHTTP.Modules.Core;
using GenHTTP.Modules.IO;

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
            var layout = Layout.Create().Add("utf8", Content.From("From GenHTTP with ❤"));

            using (var runner = TestRunner.Run(layout))
            {
                using var response = runner.GetResponse("/utf8");
                Assert.Equal("From GenHTTP with ❤", response.GetContent());
            }
        }

    }

}
