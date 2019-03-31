using System;
using System.Collections.Generic;
using System.Text;

using Xunit;
using Moq;

using GenHTTP.Api.Modules.Templating;
using GenHTTP.Api.Protocol;

namespace GenHTTP.Modules.Core.Tests
{

    public class PlaceholdersTests
    {

        [Fact]
        public void TestTemplate()
        {
            var template = Data.FromString("[Title] [Content]");
            var renderer = Placeholders.Template(template).Build();
            
            var request = Mock.Of<IRequest>();

            Assert.Equal("Hello World", renderer.Render(new TemplateModel(request, "Hello", "World")));
        }
        
    }

}
