namespace GenHTTP.Api.Content.Services;

/// <summary>
/// Adds a detailed explanation to an element of a service that goes beyond
/// the short text provided by the <see cref="SummaryAttribute" />.
/// </summary>
/// <remarks>
/// Depending on the consumer, the text may be rendered as Markdown (as for
/// descriptions within an OpenAPI specification).
/// </remarks>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Interface | AttributeTargets.Enum |
                AttributeTargets.Method | AttributeTargets.Property | AttributeTargets.Field)]
public sealed class RemarksAttribute : DocumentationAttribute
{

    #region Get-/Setters

    /// <summary>
    /// The detailed explanation of the annotated element.
    /// </summary>
    public string Text { get; }

    #endregion

    #region Initialization

    /// <summary>
    /// Adds a detailed explanation to the annotated element.
    /// </summary>
    /// <param name="text">The detailed explanation of the annotated element</param>
    public RemarksAttribute(string text)
    {
        Text = text;
    }

    #endregion

}
