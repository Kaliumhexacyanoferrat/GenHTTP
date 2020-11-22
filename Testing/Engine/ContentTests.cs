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
using GenHTTP.Modules.SinglePageApplications;
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

            public IEnumerable<ContentElement> GetContent(IRequest request) => Content.GetContent(request);

            public ValueTask<IResponse?> HandleAsync(IRequest request)
            {
                var response = request.Respond()
                              .Content(new JsonContent(Content.GetContent(request), new JsonSerializerOptions()))
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
        public void TestResources()
        {
            using var runner = TestRunner.Run(Resources.From(ResourceTree.FromAssembly("Resources"))
                                         .Add(new ContentPrinterBuilder()));

            using var response = runner.GetResponse();

            Assert.IsTrue(response.GetContent().Contains("Error.html"));
        }

        [TestMethod]
        public void TestDirectoryListing()
        {
            using var runner = TestRunner.Run(Listing.From(ResourceTree.FromAssembly("Resources"))
                                         .Add(new ContentPrinterBuilder()));

            using var response = runner.GetResponse();

            Assert.IsTrue(response.GetContent().Contains("Error.html"));
        }

        [TestMethod]
        public void TestSinglePageApplication()
        {
            using var runner = TestRunner.Run(SinglePageApplication.From(ResourceTree.FromAssembly("Resources"))
                                         .Add(new ContentPrinterBuilder()));

            using var response = runner.GetResponse();

            Assert.IsTrue(response.GetContent().Contains("Error.html"));
        }

        [TestMethod]
        public void TestWebsite()
        {
            using var runner = TestRunner.Run(WebsiteTests.GetWebsite().Add(new ContentPrinterBuilder()));

            using var response = runner.GetResponse();

            var content = response.GetContent();

            AssertX.Contains("custom.js", content);
            AssertX.Contains("custom.css", content);

            AssertX.Contains("sitemap.xml", content);
            AssertX.Contains("robots.txt", content);

            AssertX.Contains("some.txt", content);
            AssertX.Contains("favicon.ico", content);
        }

    }

}
