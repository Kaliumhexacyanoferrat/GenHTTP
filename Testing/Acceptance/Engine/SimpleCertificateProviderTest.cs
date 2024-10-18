using System.Security.Cryptography.X509Certificates;
using GenHTTP.Engine.Internal.Infrastructure.Endpoints;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSubstitute;

namespace GenHTTP.Testing.Acceptance.Engine;

[TestClass]
public sealed class SimpleCertificateProviderTest
{

    [TestMethod]
    public void TestProvider()
    {
        using var cert = Substitute.For<X509Certificate2>();

        var provider = new SimpleCertificateProvider(cert);

        Assert.IsNotNull(provider.Provide("google.com"));
    }
}
