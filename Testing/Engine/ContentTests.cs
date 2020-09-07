using System;
using System.Collections.Generic;
using System.Text.Json;

using GenHTTP.Api.Content;
using GenHTTP.Api.Protocol;

using GenHTTP.Modules.Basics;
using GenHTTP.Modules.Conversion.Providers.Json;
using GenHTTP.Modules.DirectoryBrowsing;
using GenHTTP.Modules.IO;

using GenHTTP.Testing.Acceptance.Providers;

using Xunit;

namespace GenHTTP.Testing.Acceptance.Engine
{

    public class ContentTests
    {

        #region Supporting data structures

        public class ContentPrinter : IConcern
        {

            public IHandler Content { get; }

            public IHandler Parent { get; }

            public ContentPrinter(IHandler parent, Func<IHandler, IHandler> contentFactory)
            {
                Parent = parent;
                Content = contentFactory(this);
            }

            public IEnumerable<ContentElement> GetContent(IRequest request) => Content.GetContent(request);

            public IResponse? Handle(IRequest request)
            {
                return request.Respond()
                              .Content(new JsonContent(Content.GetContent(request), new JsonSerializerOptions()))
                              .Type(ContentType.ApplicationJson)
                              .Build();
            }

        }

        public class ContentPrinterBuilder : IConcernBuilder
        {

            public IConcern Build(IHandler parent, Func<IHandler, IHandler> contentFactory)
            {
                return new ContentPrinter(parent, contentFactory);
            }

        }

        #endregion

        [Fact]
        public void TestEmbeddedResources()
        {
            using var runner = TestRunner.Run(Static.Resources("Resources").Add(new ContentPrinterBuilder()));

            using var response = runner.GetResponse();

            Assert.Contains("Error.html", response.GetContent());
        }

        [Fact]
        public void TestStaticResources()
        {
            using var runner = TestRunner.Run(Static.Files("./").Add(new ContentPrinterBuilder()));

            using var response = runner.GetResponse();

            Assert.Contains("Acceptance", response.GetContent());
        }

        [Fact]
        public void TestDirectoryListing()
        {
            using var runner = TestRunner.Run(DirectoryListing.From("./").Add(new ContentPrinterBuilder()));

            using var response = runner.GetResponse();

            Assert.Contains("Acceptance", response.GetContent());
        }

        [Fact]
        public void TestWebsite()
        {
            using var runner = TestRunner.Run(WebsiteTests.GetWebsite().Add(new ContentPrinterBuilder()));

            using var response = runner.GetResponse();

            var content = response.GetContent();

            Assert.Contains("custom.js", content);
            Assert.Contains("custom.css", content);

            Assert.Contains("sitemap.xml", content);
            Assert.Contains("robots.txt", content);

            Assert.Contains("some.txt", content);
            Assert.Contains("favicon.ico", content);
        }

    }

}
