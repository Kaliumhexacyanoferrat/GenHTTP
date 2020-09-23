using System.Net;

using Xunit;

namespace GenHTTP.Testing.Acceptance.Engine
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
