using GenHTTP.Modules.ErrorHandling.Provider;

namespace GenHTTP.Modules.ErrorHandling
{

    public static class ErrorHandler
    {

        public static ErrorHandlingProviderBuilder Default() => new ErrorHandlingProviderBuilder();

    }

}
