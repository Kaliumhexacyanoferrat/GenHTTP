using System;
using Xunit;

namespace GenHTTP.Core.Tests
{

    public class UnitTest1
    {

        [Fact]
        public void Test1()
        {
            Assert.NotNull(Server.Create());
        }

    }

}
