namespace GenHTTP.Api.Content.Services;

/// <summary>
/// Declares a header that is read from the request by an operation.
/// </summary>
/// <remarks>
/// Placed on a service class, the header applies to all operations
/// of the service.
/// </remarks>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Interface | AttributeTargets.Method, AllowMultiple = true)]
public sealed class RequestHeaderAttribute : DocumentationAttribute
{

    #region Get-/Setters

    /// <summary>
    /// The name of the header (e.g. "X-Request-Id").
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// Describes the value expected to be sent by the client.
    /// </summary>
    public string? Description { get; }

    /// <summary>
    /// Whether the operation requires the client to send this header.
    /// </summary>
    public bool Required { get; set; }

    /// <summary>
    /// The type of the value of the header, if it is not a plain string.
    /// </summary>
    public Type? Type { get; set; }

    #endregion

    #region Initialization

    /// <summary>
    /// Declares a header that is read from the request by the annotated operation.
    /// </summary>
    /// <param name="name">The name of the header (e.g. "X-Request-Id")</param>
    /// <param name="description">Describes the value expected to be sent by the client</param>
    public RequestHeaderAttribute(string name, string? description = null)
    {
        Name = name;
        Description = description;
    }

    #endregion

}
