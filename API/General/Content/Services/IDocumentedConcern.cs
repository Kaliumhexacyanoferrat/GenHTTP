namespace GenHTTP.Api.Content.Services;

/// <summary>
/// A concern that documents the functionality it adds to the content
/// it wraps (such as the headers it reads or the responses it may generate),
/// so it can be presented to the consumers of the API.
/// </summary>
/// <remarks>
/// The documentation added by the concern is treated as if it was declared on the
/// service class of the operation. Documentation declared on the services themselves
/// and documentation of concerns closer to the content take precedence. Concerns may use
/// <see cref="ResponseAttribute" />, <see cref="RequestHeaderAttribute" /> and
/// <see cref="ResponseHeaderAttribute" /> instances to document themselves, other kinds of
/// documentation are ignored.
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
