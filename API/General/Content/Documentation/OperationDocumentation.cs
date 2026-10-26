using GenHTTP.Api.Content.Documentation.Security;
using GenHTTP.Api.Protocol;

namespace GenHTTP.Api.Content.Documentation;

/// <summary>
/// Collects the documentation a component (such as a concern) adds to a single
/// operation of the content it is applied to (see <see cref="IDocumentedConcern" />).
/// </summary>
public sealed class OperationDocumentation
{
    private readonly List<DocumentationAttribute> _entries = [];

    private readonly List<AuthenticationRequirement> _authentication = [];

    #region Get-/Setters

    /// <summary>
    /// The HTTP verb of the operation to be documented.
    /// </summary>
    public RequestMethod Method { get; }

    /// <summary>
    /// The responses and headers added to the operation.
    /// </summary>
    public IReadOnlyList<DocumentationAttribute> Entries => _entries;

    /// <summary>
    /// The authentication required to access the operation. All of
    /// the requirements need to be satisfied by the client.
    /// </summary>
    public IReadOnlyList<AuthenticationRequirement> Authentication => _authentication;

    #endregion

    #region Initialization

    /// <summary>
    /// Creates a new, empty documentation for an operation.
    /// </summary>
    /// <param name="method">The HTTP verb of the operation to be documented</param>
    public OperationDocumentation(RequestMethod method)
    {
        Method = method;
    }

    #endregion

    #region Functionality

    /// <summary>
    /// Declares a response that may be generated for the operation.
    /// </summary>
    /// <param name="status">The status of the response</param>
    /// <param name="description">Describes the circumstances the response is generated in</param>
    /// <param name="type">The type of the content sent with the response, if any</param>
    /// <param name="contentType">The content type of the response (e.g. "text/plain"), if known</param>
    /// <remarks>
    /// See <see cref="ResponseAttribute" /> for details.
    /// </remarks>
    public OperationDocumentation Response(ResponseStatus status, string? description = null, Type? type = null, string? contentType = null)
    {
        _entries.Add(new ResponseAttribute(status, description) { Type = type, ContentType = contentType });
        return this;
    }

    /// <summary>
    /// Declares a header that is read from the request.
    /// </summary>
    /// <param name="name">The name of the header (e.g. "X-Request-Id")</param>
    /// <param name="description">Describes the value expected to be sent by the client</param>
    /// <param name="required">Whether the client is required to send this header</param>
    /// <param name="type">The type of the value of the header, if it is not a plain string</param>
    public OperationDocumentation RequestHeader(string name, string? description = null, bool required = false, Type? type = null)
    {
        _entries.Add(new RequestHeaderAttribute(name, description) { Required = required, Type = type });
        return this;
    }

    /// <summary>
    /// Declares a header that is sent with the response.
    /// </summary>
    /// <param name="name">The name of the header (e.g. "Retry-After")</param>
    /// <param name="description">Describes the value sent with the header</param>
    /// <param name="status">The status of the response the header is sent with, or null for every successful response</param>
    /// <param name="type">The type of the value of the header, if it is not a plain string</param>
    /// <remarks>
    /// See <see cref="ResponseHeaderAttribute" /> for details.
    /// </remarks>
    public OperationDocumentation ResponseHeader(string name, string? description = null, ResponseStatus? status = null, Type? type = null)
    {
        var header = (status != null) ? new ResponseHeaderAttribute(status.Value, name, description) : new ResponseHeaderAttribute(name, description);

        header.Type = type;

        _entries.Add(header);
        return this;
    }

    /// <summary>
    /// Declares that the client needs to authenticate with any one
    /// of the given schemes to access the operation.
    /// </summary>
    /// <param name="anyOf">The schemes the client may choose from</param>
    /// <remarks>
    /// If called multiple times, all of the requirements need to be satisfied.
    /// </remarks>
    public OperationDocumentation RequireAuthentication(params SecurityScheme[] anyOf)
    {
        if (anyOf.Length == 0)
        {
            throw new ArgumentException("At least one security scheme is required", nameof(anyOf));
        }

        _authentication.Add(new AuthenticationRequirement(anyOf));
        return this;
    }

    #endregion

}
