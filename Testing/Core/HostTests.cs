using System;
using System.Net;
using System.Collections.Generic;
using System.Text;

using Xunit;

using GenHTTP.Testing.Acceptance.Domain;

namespace GenHTTP.Testing.Acceptance.Core
{
    
    public class HostTests
    {
        
        [Fact]
        public void TestStart()
        {
            using var runner = new TestRunner();

            runner.Host.Start();

            using var response = runner.GetResponse();

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public void TestRestart()
        {
            using var runner = new TestRunner();

            runner.Host.Restart();

            using var response = runner.GetResponse();

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

    }

}
