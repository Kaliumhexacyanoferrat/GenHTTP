using GenHTTP.Api.Content;

using GenHTTP.Modules.ErrorHandling;
using GenHTTP.Modules.ErrorHandling.Provider;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using NSubstitute;

namespace GenHTTP.Testing.Acceptance.Utilities;

internal static class Chain
{

    public static void Works<T>(IHandlerBuilder<T> builder) where T : IHandlerBuilder<T>
    {
        builder.Add(ErrorHandler.Html());

        Assert.IsTrue(builder.Build(Substitute.For<IHandler>()) is ErrorSentry<Exception>);
    }

}
