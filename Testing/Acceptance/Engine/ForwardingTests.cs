using System.Linq;
using System.Threading.Tasks;

using GenHTTP.Api.Protocol;
using GenHTTP.Modules.Functional;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GenHTTP.Testing.Acceptance.Engine;

[TestClass]
public sealed class ForwardingTests
{

    [TestMethod]
    public async Task TestModern()
    {
            var responder = Inline.Create().Get((IRequest request) => $"{request.Client}");

            using var host = TestHost.Run(responder);

            var request = host.GetRequest();

            request.Headers.Add("Forwarded", "for=85.192.1.5; host=google.com; proto=https");

            using var response = await host.GetResponseAsync(request);

            Assert.AreEqual("ClientConnection { IPAddress = 85.192.1.5, Protocol = HTTPS, Host = google.com }", await response.GetContentAsync());
        }

    [TestMethod]
    public async Task TestLegacy()
    {
            var responder = Inline.Create().Get((IRequest request) => $"{request.Client}");

            using var host = TestHost.Run(responder);

            var request = host.GetRequest();

            request.Headers.Add("X-Forwarded-For", "85.192.1.5");
            request.Headers.Add("X-Forwarded-Host", "google.com");
            request.Headers.Add("X-Forwarded-Proto", "http");

            using var response = await host.GetResponseAsync(request);

            Assert.AreEqual("ClientConnection { IPAddress = 85.192.1.5, Protocol = HTTP, Host = google.com }", await response.GetContentAsync());
        }

    [TestMethod]
    public async Task TestBoth()
    {
            var responder = Inline.Create().Get((IRequest request) => $"{request.Client}");

            using var host = TestHost.Run(responder);

            var request = host.GetRequest();

            request.Headers.Add("Forwarded", "for=85.192.1.1; host=google.com; proto=https");

            request.Headers.Add("X-Forwarded-For", "85.192.1.2");
            request.Headers.Add("X-Forwarded-Host", "google2.com");
            request.Headers.Add("X-Forwarded-Proto", "http");

            using var response = await host.GetResponseAsync(request);

            Assert.AreEqual("ClientConnection { IPAddress = 85.192.1.1, Protocol = HTTPS, Host = google.com }", await response.GetContentAsync());
        }

    [TestMethod]
    public async Task TestInvalid()
    {
            var responder = Inline.Create().Get((IRequest request) => $"{request.Forwardings.First().ToString()}");

            using var host = TestHost.Run(responder);

            var request = host.GetRequest();

            request.Headers.Add("Forwarded", "for=google.com; host=google.com; proto=google.com");

            using var response = await host.GetResponseAsync(request);

            Assert.AreEqual("Forwarding { For = , Host = google.com, Protocol =  }", await response.GetContentAsync());
        }

}
