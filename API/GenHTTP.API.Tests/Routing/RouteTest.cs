using System;
using System.Collections.Generic;
using System.Text;

using Xunit;

using GenHTTP.Api.Routing;

namespace GenHTTP.Api.Tests.Routing
{

    public class RouteTest
    {

        [Fact]
        public void TestRelation()
        {
            Assert.Equal("./", Route.GetRelation(0));
            Assert.Equal("../../", Route.GetRelation(2));
        }
        
        [Fact]
        public void TestGetSegement()
        {
            Assert.Equal("test", Route.GetSegment("/test"));
            Assert.Equal("test", Route.GetSegment("/test/"));
            Assert.Equal("test", Route.GetSegment("test/"));
            Assert.Equal("test", Route.GetSegment("test"));
        }

    }

}
