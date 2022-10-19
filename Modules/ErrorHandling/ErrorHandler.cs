using System;

using GenHTTP.Api.Content;

using GenHTTP.Modules.ErrorHandling.Provider;

namespace GenHTTP.Modules.ErrorHandling
{

    public static class ErrorHandler
    {

        /// <summary>
        /// The default error handler used by the server to
        /// render error pages.
        /// </summary>
        /// <remarks>
        /// By default, server errors will be rendered to HTML.
        /// </remarks>
        /// <returns>The default error handler</returns>
        public static ErrorHandlingProviderBuilder<Exception> Default() => Html();

        /// <summary>
        /// An error handler which will render exceptions into
        /// HTML using the current template and IErrorRenderer.
        /// </summary>
        /// <returns>An HTML error handler</returns>
        public static ErrorHandlingProviderBuilder<Exception> Html() => new(new HtmlErrorHandler());

        /// <summary>
        /// Creates an error handling concern which will use
        /// the specified error handler to map exceptions
        /// into a HTTP response.
        /// </summary>
        /// <typeparam name="T">The type of exceptions to be catched</typeparam>
        /// <param name="handler">The handler to use for exception mapping</param>
        /// <returns>The newly generated concern</returns>
        public static ErrorHandlingProviderBuilder<T> With<T>(IErrorHandler<T> handler) where T : Exception => new(handler);

    }

}
