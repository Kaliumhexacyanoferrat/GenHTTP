using System;

using GenHTTP.Api.Content;
using GenHTTP.Modules.ErrorHandling.Provider;

namespace GenHTTP.Modules.ErrorHandling
{

    public static class ErrorHandler
    {

        public static ErrorHandlingProviderBuilder<Exception> Default() => Html();

        public static ErrorHandlingProviderBuilder<Exception> Html() => new();

        public static ErrorHandlingProviderBuilder<T> With<T>(IErrorHandler<T> handler) where T : Exception => new(handler) ;

    }

}
