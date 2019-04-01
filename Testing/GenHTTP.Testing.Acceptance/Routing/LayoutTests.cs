using System;
using System.Collections.Generic;
using System.Text;
using System.Net;

using Xunit;

using GenHTTP.Testing.Acceptance.Domain;
using GenHTTP.Modules.Core;

namespace GenHTTP.Testing.Acceptance.Routing
{

    public class LayoutTests
    {

        /// <summary>
        /// As a developer I can define the default route to be devlivered.
        /// </summary>
        [Fact]
        public void TestGetIndex()
        {
            var layout = Layout.Create()
                               .Add("index", Content.From("Hello World!"), true);
            
            using (var runner = TestRunner.Run(layout))
            {
                using var response = runner.GetResponse();

                Assert.Equal(HttpStatusCode.OK, response.StatusCode);
                Assert.Equal("Hello World!", response.GetContent());
            }
        }

    }

}
