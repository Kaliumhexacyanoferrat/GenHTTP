using System.Net;
using GenHTTP.Modules.IO;
using GenHTTP.Modules.Layouting;
using GenHTTP.Modules.Markdown;
using Xunit;

namespace GenHTTP.Testing.Acceptance.Modules
{
    public class MarkdownTests
    {
        [Fact]
        public void TestGetDownloadFromResource()
        {
            var layout = Layout.Create()
                .Add("legal", ModMarkdown.Page(Data.FromFile("./Resources/legal.md")));

            using (var runner = TestRunner.Run(layout))
            {
                var response = runner.GetResponse("/legal");
                Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            }
        }
    }
}