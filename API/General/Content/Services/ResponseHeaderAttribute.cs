using GenHTTP.Api.Protocol;

namespace GenHTTP.Api.Content.Services;

/// <summary>
/// Declares a header that is sent with the response of an operation.
/// </summary>
/// <remarks>
/// If no status is given, the header will be added to all successful
/// responses (2xx) of the operation. Otherwise, it will be added to the
/// response with the given status, if the operation declares such a response
/// (see <see cref="ResponseAttribute" />). Placed on a service class, the header
/// applies to all operations of the service.
/// </remarks>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Interface | AttributeTargets.Method, AllowMultiple = true)]
public sealed class ResponseHeaderAttribute : DocumentationAttribute
{

    #region Get-/Setters

    /// <summary>
    /// The status of the response the header is sent with, or null
    /// if the header is sent with every successful response.
    /// </summary>
    public ResponseStatus? Status { get; }

    /// <summary>
    /// The name of the header (e.g. "Location").
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// Describes the value sent with the header.
    /// </summary>
    public string? Description { get; }

    /// <summary>
    /// The type of the value of the header, if it is not a plain string.
    /// </summary>
    public Type? Type { get; set; }

    #endregion

    #region Initialization

    /// <summary>
    /// Declares a header that is sent with every successful response of the annotated operation.
    /// </summary>
    /// <param name="name">The name of the header (e.g. "Location")</param>
    /// <param name="description">Describes the value sent with the header</param>
    public ResponseHeaderAttribute(string name, string? description = null)
    {
        Name = name;
        Description = description;
    }

    /// <summary>
    /// Declares a header that is sent with the response of the given status.
    /// </summary>
    /// <param name="status">The status of the response the header is sent with</param>
    /// <param name="name">The name of the header (e.g. "Retry-After")</param>
    /// <param name="description">Describes the value sent with the header</param>
    public ResponseHeaderAttribute(ResponseStatus status, string name, string? description = null) : this(name, description)
    {
        Status = status;
    }

    #endregion

}
