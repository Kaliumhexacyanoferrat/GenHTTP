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
        /// By default, server errors will be rendered into
        /// a HTML template.
        /// </remarks>
        /// <returns>The default error handler</returns>
        public static ErrorSentryBuilder<Exception> Default() => Html();

        /// <summary>
        /// An error handler which will render exceptions into
        /// HTML using the current template and IErrorRenderer.
        /// </summary>
        /// <returns>An HTML error handler</returns>
        public static ErrorSentryBuilder<Exception> Html() => From(new HtmlErrorMapper());

        /// <summary>
        /// Creates an error handling concern which will use
        /// the specified error mapper to convert exceptions
        /// to HTTP responses.
        /// </summary>
        /// <typeparam name="T">The type of exceptions to be catched</typeparam>
        /// <param name="mapper">The mapper to use for exception mapping</param>
        /// <returns>The newly generated concern</returns>
        public static ErrorSentryBuilder<T> From<T>(IErrorMapper<T> mapper) where T : Exception => new(mapper);

    }

}
