using GenHTTP.Modules.Conversion.Serializers;
using GenHTTP.Modules.ErrorHandling.Provider;

namespace GenHTTP.Modules.ErrorHandling;

public static class ErrorHandler
{

    /// <summary>
    /// The default error handler used by the server to
    /// render error pages.
    /// </summary>
    /// <remarks>
    /// By default, server errors will be rendered into
    /// structured responses.
    /// </remarks>
    /// <returns>The default error handler</returns>
    public static ErrorSentryBuilder<Exception> Default() => Structured();

    /// <summary>
    /// Ans error handler which will render exceptions into
    /// structured error objects serialized to the format
    /// requested by the client (e.g. JSON or XML).
    /// </summary>
    /// <param name="serialization">The serialization configuration to be used</param>
    /// <returns>A structured error handler</returns>
    public static ErrorSentryBuilder<Exception> Structured(SerializationBuilder? serialization = null) => From(new StructuredErrorMapper(serialization?.Build()));

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
