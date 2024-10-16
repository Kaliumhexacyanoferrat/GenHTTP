﻿using GenHTTP.Api.Content;
using GenHTTP.Api.Protocol;
using GenHTTP.Modules.Basics;
using GenHTTP.Modules.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GenHTTP.Testing.Acceptance.Engine;

[TestClass]
public sealed class CookieTests
{

    /// <summary>
    /// As a developer, I want to be able to set cookies to be accepted by the browser.
    /// </summary>
    [TestMethod]
    public async Task TestCookiesCanBeReturned()
    {
        using var runner = TestHost.Run(new TestProvider().Wrap());

        using var response = await runner.GetResponseAsync();

        Assert.AreEqual("TestCookie=TestValue; Max-Age=86400; Path=/", response.GetHeader("Set-Cookie"));
    }

    /// <summary>
    /// As a developer, I want to be able to read cookies from the client.
    /// </summary>
    [TestMethod]
    public async Task TestCookiesCanBeRead()
    {
        var provider = new TestProvider();

        using var runner = TestHost.Run(provider.Wrap());

        var request = runner.GetRequest();
        request.Headers.Add("Cookie", "1=2; 3=4");

        using var _ = await runner.GetResponseAsync(request);

        Assert.AreEqual("4", provider.Cookies?["3"].Value);
    }

    private class TestProvider : IHandler
    {

        public ICookieCollection? Cookies { get; private set; }

        public ValueTask PrepareAsync() => ValueTask.CompletedTask;

        public IHandler Parent => throw new NotSupportedException();

        public ValueTask<IResponse?> HandleAsync(IRequest request)
        {
            Cookies = request.Cookies;

            return request.Respond()
                          .Cookie(new Cookie("TestCookie", "TestValue", 86400))
                          .Content("I ❤ Cookies!")
                          .Type(ContentType.TextHtml)
                          .BuildTask();

        }
    }
}
