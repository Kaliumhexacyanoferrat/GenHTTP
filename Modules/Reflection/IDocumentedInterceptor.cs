using GenHTTP.Api.Content.Documentation;

namespace GenHTTP.Modules.Reflection;

/// <summary>
/// An interceptor that documents the functionality it adds to the
/// operation it has been placed on (such as the responses it may generate),
/// so it can be presented to the consumers of the API.
/// </summary>
/// <remarks>
/// Responses and headers added by the interceptor take precedence over the
/// documentation declared on the service class or by concerns, but not over
/// the documentation declared on the method itself.
/// </remarks>
public interface IDocumentedInterceptor : IOperationInterceptor
{

    /// <summary>
    /// Invoked for every HTTP verb supported by the operation, allowing the
    /// interceptor to document the functionality it adds to the operation.
    /// </summary>
    /// <param name="operation">The operation to add the documentation to</param>
    void AddDocumentation(OperationDocumentation operation);

}
