using GenHTTP.Modules.IO;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using Xunit;

namespace GenHTTP.Testing.Acceptance.Modules.IO
{
    
    public class ContentTests
    {

        [Fact]
        public void TestContent()
        {
            using var runner = TestRunner.Run(Content.From(Resource.FromString("Hello World!")));

            using var response = runner.GetResponse();

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal("Hello World!", response.GetContent());
        }

        [Fact]
        public void TestContentIgnoresRouting()
        {
            using var runner = TestRunner.Run(Content.From(Resource.FromString("Hello World!")));

            using var response = runner.GetResponse("/some/path");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

    }

}
