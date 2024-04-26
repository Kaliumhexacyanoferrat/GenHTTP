using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using GenHTTP.Api.Protocol;

using GenHTTP.Modules.IO;
using GenHTTP.Modules.Minification;

namespace GenHTTP.Testing.Acceptance.Modules.Minification
{

    [TestClass]
    public sealed class MinificationTests
    {

        [TestMethod]
        public async Task TestNoMinificationInDevelopmentMode()
        {
            var original = @"function myFunction() {}";

            var result = await Run(original, ContentType.ApplicationJavaScript, developmentMode: true);

            Assert.AreEqual(result.Length, original.Length);
        }

        [TestMethod]
        public async Task TestJSMinification()
        {
            var original = @"
                function myFunction() {
                    alert('minify me!?');
                };

                myFunction();
            ";

            var result = await Run(original, ContentType.ApplicationJavaScript);

            AssertX.Contains("minify me", result);

            Assert.IsTrue(result.Length < original.Length);
        }

        [TestMethod]
        public async Task TestCssMinification()
        {
            var original = @"
                body {
                    text-color: white;
                }
            ";

            var result = await Run(original, ContentType.TextCss);

            AssertX.Contains("text-color", result);

            Assert.IsTrue(result.Length < original.Length);
        }

        [TestMethod]
        public async Task TestHtmlMinification()
        {
            var original = @"
                <!DOCTYPE html>
                <html lang=""en"">
                <body>Hello World!</body>
                </html>
            ";

            var result = await Run(original, ContentType.TextHtml);

            AssertX.Contains("Hello World!", result);

            Assert.IsTrue(result.Length < original.Length);
        }

        [TestMethod]
        public async Task TestNoMinification()
        {
            var original = @"
                One
                Two
                Three
            ";

            var result = await Run(original, ContentType.TextPlain);

            AssertX.Contains("Two", result);

            Assert.AreEqual(result.Length, original.Length);
        }

        [TestMethod]
        public async Task TestIgnoreStrategy()
        {
            // we expect the syntax error on the last line to be ignored but the text color to be minified
            var original = @"
                .my { text-color: white; }
                body { 
            ";

            var response = await RunErrorHandling(original, ContentType.TextCss, MinificationErrors.Ignore);

            await response.AssertStatusAsync(HttpStatusCode.OK);

            AssertX.Contains("#fff", await response.GetContentAsync());
        }

        [TestMethod]
        public async Task TestOriginalStrategy()
        {
            // we expect the original to be served, so no minimalization of the color
            var original = @"
                .my { text-color: white; }
                body { 
            ";

            var response = await RunErrorHandling(original, ContentType.TextCss, MinificationErrors.ServeOriginal);

            await response.AssertStatusAsync(HttpStatusCode.OK);

            AssertX.Contains("white", await response.GetContentAsync());
        }

        [TestMethod]
        public async Task TestThrowStrategy()
        {
            // this should plainly throw, causing the request to abort
            var original = @"
                .my { text-color: white; }
                body { 
            ";

            await Assert.ThrowsExceptionAsync<HttpRequestException>(async () =>
            {
                await RunErrorHandling(original, ContentType.TextCss, MinificationErrors.Throw);
            });
        }
                
        private static async Task<string> Run(string original, ContentType contentType, bool developmentMode = false)
        {
            var content = Content.From(Resource.FromString(original).Type(new FlexibleContentType(contentType)))
                                 .Minification();

            using var runner = TestHost.Run(content, development: developmentMode);

            using var response = await runner.GetResponseAsync();

            await response.AssertStatusAsync(HttpStatusCode.OK);

            return await response.GetContentAsync();
        }

        private static async Task<HttpResponseMessage> RunErrorHandling(string original, ContentType contentType, MinificationErrors strategy, bool developmentMode = false)
        {
            var content = Content.From(Resource.FromString(original).Type(new FlexibleContentType(contentType)))
                                 .Add(Minify.Default().ErrorHandling(strategy));

            using var runner = TestHost.Run(content, development: developmentMode);

            return await runner.GetResponseAsync();
        }

    }

}
