using Xunit;

using GenHTTP.Modules.Core;
using GenHTTP.Testing.Acceptance.Domain;

namespace GenHTTP.Testing.Acceptance.Core
{

    public class EncodingTests
    {

        /// <summary>
        /// As a developer, I want UTF-8 to be my default encoding.
        /// </summary>
        [Fact]
        public void TestUtf8DefaultEncoding()
        {
            var layout = Layout.Create().File("utf8", Content.From("From GenHTTP with ❤"));

            using (var runner = TestRunner.Run(layout))
            {
                using var response = runner.GetResponse("/utf8");
                Assert.Equal("From GenHTTP with ❤", response.GetContent());
            }
        }

    }

}
