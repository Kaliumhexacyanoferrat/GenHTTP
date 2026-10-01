using GenHTTP.Api.Protocol;

namespace GenHTTP.Api.Content.Services;

/// <summary>
/// Collects the documentation a concern adds to a single operation
/// of the content it wraps (see <see cref="IDocumentedConcern" />).
/// </summary>
public sealed class OperationDocumentation
{
    private readonly List<DocumentationAttribute> _entries = [];

    #region Get-/Setters

    /// <summary>
    /// The HTTP verb of the operation to be documented.
    /// </summary>
    public RequestMethod Method { get; }

    /// <summary>
    /// The documentation added to the operation.
    /// </summary>
    public IReadOnlyList<DocumentationAttribute> Entries => _entries;

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
    /// Adds the given documentation to the operation.
    /// </summary>
    /// <param name="documentation">The documentation to be added</param>
    public OperationDocumentation Add(DocumentationAttribute documentation)
    {
        _entries.Add(documentation);
        return this;
    }

    #endregion

}
