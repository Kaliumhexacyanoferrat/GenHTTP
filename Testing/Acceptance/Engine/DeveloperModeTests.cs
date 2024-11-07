﻿using GenHTTP.Api.Content;
using GenHTTP.Api.Protocol;
using GenHTTP.Modules.Layouting;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GenHTTP.Testing.Acceptance.Engine;

[TestClass]
public sealed class DeveloperModeTests
{

    /// <summary>
    /// As a developer of a web project, I would like to see exceptions rendered
    /// in the browser, so that I can trace an error more quickly
    /// </summary>
    [TestMethod]
    [MultiEngineTest]
    public async Task TestExceptionsWithTrace(TestEngine engine)
    {
        using var runner = new TestHost(Layout.Create().Build(), engine: engine);

        var router = Layout.Create().Index(new ThrowingProvider().Wrap());

        runner.Host.Handler(router).Development().Start();

        using var response = await runner.GetResponseAsync();

        Assert.IsTrue((await response.GetContentAsync()).Contains("at GenHTTP"));
    }

    /// <summary>
    /// As a devops member, I do not want an web application to leak internal
    /// implementation detail with exception messages
    /// </summary>
    [TestMethod]
    [MultiEngineTest]
    public async Task TestExceptionsWithNoTrace(TestEngine engine)
    {
        var router = Layout.Create().Index(new ThrowingProvider().Wrap());

        using var runner = TestHost.Run(router, development: false, engine: engine);

        using var response = await runner.GetResponseAsync();

        Assert.IsFalse((await response.GetContentAsync()).Contains("at GenHTTP"));
    }

    private class ThrowingProvider : IHandler
    {

        public ValueTask PrepareAsync() => ValueTask.CompletedTask;

        public IHandler Parent => throw new NotImplementedException();

        public ValueTask<IResponse?> HandleAsync(IRequest request) => throw new InvalidOperationException("Nope!");
    }
}
