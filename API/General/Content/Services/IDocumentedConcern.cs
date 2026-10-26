namespace GenHTTP.Api.Content.Services;

/// <summary>
/// A concern that documents the functionality it adds to the content
/// it wraps (such as the headers it reads, the responses it may generate or
/// the authentication it requires), so it can be presented to the consumers of the API.
/// </summary>
/// <remarks>
/// Responses and headers added by the concern are treated as if they were declared on the
/// service class of the operation. Documentation declared on the services themselves
/// and documentation of concerns closer to the content take precedence. Required authentication
/// adds up instead, as the client needs to pass every concern on the way to the operation.
/// </remarks>
public interface IDocumentedConcern : IConcern
{

    /// <summary>
    /// Invoked for every operation provided by the wrapped content, allowing the concern
    /// to document the functionality it adds to this specific operation.
    /// </summary>
    /// <param name="operation">The operation to add the documentation to</param>
    void AddDocumentation(OperationDocumentation operation);

}
