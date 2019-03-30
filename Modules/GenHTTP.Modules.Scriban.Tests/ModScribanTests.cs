using System;
using System.Collections.Generic;
using System.Text;

using Xunit;
using Moq;

using GenHTTP.Api.Protocol;
using GenHTTP.Api.Modules.Templating;

using GenHTTP.Modules.Core;

namespace GenHTTP.Modules.Scriban.Tests
{

    public class ModScribanTests
    {

        [Fact]
        public void TestTemplate()
        {
            var template = Data.FromString("{{ title }} {{ content }}");
            var renderer = ModScriban.Template(template).Build();

            var request = Mock.Of<IHttpRequest>();
            var response = Mock.Of<IHttpResponse>();

            Assert.Equal("Hello World", renderer.Render(new TemplateModel(request, response, "Hello", "World")));
        }

    }

}
