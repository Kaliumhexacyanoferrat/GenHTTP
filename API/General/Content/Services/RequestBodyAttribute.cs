namespace GenHTTP.Api.Content.Services;

/// <summary>
/// Describes the body expected by an operation of a service.
/// </summary>
/// <remarks>
/// The request body is usually derived from the signature of the method
/// implementing the operation. This attribute allows to describe the body
/// if the method reads it on its own (e.g. from the request or a stream),
/// or to restrict the content types that are accepted. Declaring multiple
/// bodies with different content types will merge them.
/// </remarks>
[AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
public sealed class RequestBodyAttribute : DocumentationAttribute
{

    #region Get-/Setters

    /// <summary>
    /// Describes the content to be sent by the client.
    /// </summary>
    public string? Description { get; }

    /// <summary>
    /// The type of the content expected by the operation, if it
    /// cannot be derived from the method signature.
    /// </summary>
    public Type? Type { get; set; }

    /// <summary>
    /// The content type expected by the operation (e.g. "application/zip"), if it is
    /// not determined by the serialization formats supported by the service.
    /// </summary>
    public string? ContentType { get; set; }

    #endregion

    #region Initialization

    /// <summary>
    /// Describes the body expected by the annotated operation.
    /// </summary>
    /// <param name="description">Describes the content to be sent by the client</param>
    public RequestBodyAttribute(string? description = null)
    {
        Description = description;
    }

    #endregion

}
