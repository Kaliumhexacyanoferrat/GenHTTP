namespace GenHTTP.Api.Content.Services;

/// <summary>
/// Briefly describes an element of a service, so it can be presented
/// to the consumers of the API (e.g. in a generated OpenAPI specification).
/// </summary>
/// <remarks>
/// On a method, the text describes the operation. On a parameter, it describes the
/// argument expected by the operation and on the return value (<c>[return: Summary("...")]</c>)
/// it describes the successful response. On a service class, it describes the group the
/// operations of the service are listed under. On a model type or one of its properties,
/// it describes the generated schema. For positional records, the attribute can be placed
/// on the parameter of the record and does not need to target the generated property.
/// </remarks>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Interface | AttributeTargets.Enum |
                AttributeTargets.Method | AttributeTargets.Parameter | AttributeTargets.ReturnValue |
                AttributeTargets.Property | AttributeTargets.Field)]
public sealed class SummaryAttribute : DocumentationAttribute
{

    #region Get-/Setters

    /// <summary>
    /// The text describing the annotated element.
    /// </summary>
    public string Text { get; }

    #endregion

    #region Initialization

    /// <summary>
    /// Briefly describes the annotated element.
    /// </summary>
    /// <param name="text">The text describing the annotated element</param>
    public SummaryAttribute(string text)
    {
        Text = text;
    }

    #endregion

}
