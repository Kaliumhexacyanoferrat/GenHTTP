using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;

using GenHTTP.Api.Content;
using GenHTTP.Api.Protocol;

using GenHTTP.Modules.Basics;
using GenHTTP.Modules.Conversion.Providers.Json;
using GenHTTP.Modules.DirectoryBrowsing;
using GenHTTP.Modules.IO;
using GenHTTP.Modules.Robots;
using GenHTTP.Modules.SinglePageApplications;
using GenHTTP.Modules.Sitemaps;

using GenHTTP.Testing.Acceptance.Providers;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GenHTTP.Testing.Acceptance.Engine
{

    [TestClass]
    public sealed class ContentTests
    {

        #region Supporting data structures

        public sealed class ContentPrinter : IConcern
        {

            public IHandler Content { get; }

            public IHandler Parent { get; }

            public ContentPrinter(IHandler parent, Func<IHandler, IHandler> contentFactory)
            {
                Parent = parent;
                Content = contentFactory(this);
            }

            public ValueTask PrepareAsync() => Content.PrepareAsync();

            public IAsyncEnumerable<ContentElement> GetContentAsync(IRequest request) => Content.GetContentAsync(request);

            public ValueTask<IResponse?> HandleAsync(IRequest request)
            {
                var response = request.Respond()
                              .Content(new JsonContent(Content.GetContentAsync(request), new JsonSerializerOptions()))
                              .Type(ContentType.ApplicationJson)
                              .Build();

                return new ValueTask<IResponse?>(response);
            }

        }

        public sealed class ContentPrinterBuilder : IConcernBuilder
        {

            public IConcern Build(IHandler parent, Func<IHandler, IHandler> contentFactory)
            {
                return new ContentPrinter(parent, contentFactory);
            }

        }

        #endregion

        [TestMethod]
        public async Task TestResources()
        {
            using var runner = TestRunner.Run(Resources.From(ResourceTree.FromAssembly("Resources"))
                                         .Add(new ContentPrinterBuilder()));

            using var response = await runner.GetResponse();

            Assert.IsTrue((await response.GetContent()).Contains("Error.html"));
        }

        [TestMethod]
        public async Task TestDirectoryListing()
        {
            using var runner = TestRunner.Run(Listing.From(ResourceTree.FromAssembly("Resources"))
                                         .Add(new ContentPrinterBuilder()));

            using var response = await runner.GetResponse();

            Assert.IsTrue((await response.GetContent()).Contains("Error.html"));
        }

        [TestMethod]
        public async Task TestSinglePageApplication()
        {
            using var runner = TestRunner.Run(SinglePageApplication.From(ResourceTree.FromAssembly("Resources"))
                                         .Add(new ContentPrinterBuilder()));

            using var response = await runner.GetResponse();

            Assert.IsTrue((await response.GetContent()).Contains("Error.html"));
        }

        [TestMethod]
        public async Task TestWebsite()
        {
            using var runner = TestRunner.Run(WebsiteTests.GetWebsite().Add(new ContentPrinterBuilder()));

            using var response = await runner.GetResponse();

            var content = await response.GetContent();

            AssertX.Contains("custom.js", content);
            AssertX.Contains("custom.css", content);

            AssertX.Contains(Sitemap.FILE_NAME, content);
            AssertX.Contains(BotInstructions.FILE_NAME, content);

            AssertX.Contains("some.txt", content);
            AssertX.Contains("favicon.ico", content);
        }

    }

}
