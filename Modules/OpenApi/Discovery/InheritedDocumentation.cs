using GenHTTP.Api.Content.Services;
using GenHTTP.Api.Protocol;

namespace GenHTTP.Modules.OpenApi.Discovery;

/// <summary>
/// The concerns that wrap the handler currently explored and are able
/// to document their functionality (see <see cref="IDocumentedConcern" />).
/// </summary>
/// <remarks>
/// Immutable, so explorers can pass an extended instance to the content of
/// a concern without affecting the siblings of the concern.
/// </remarks>
public sealed class InheritedDocumentation
{

    #region Get-/Setters

    /// <summary>
    /// An instance without any documentation, used to start exploring.
    /// </summary>
    public static InheritedDocumentation Empty { get; } = new(null, null);

    private InheritedDocumentation? Outer { get; }

    private IDocumentedConcern? Concern { get; }

    #endregion

    #region Initialization

    private InheritedDocumentation(InheritedDocumentation? outer, IDocumentedConcern? concern)
    {
        Outer = outer;
        Concern = concern;
    }

    #endregion

    #region Functionality

    /// <summary>
    /// Creates the documentation to be applied to the content of the given concern.
    /// </summary>
    /// <param name="concern">The concern whose content is about to be explored</param>
    /// <returns>The documentation to explore the content of the concern with</returns>
    public InheritedDocumentation Push(IDocumentedConcern concern) => new(this, concern);

    /// <summary>
    /// Asks the concerns to document the given operation.
    /// </summary>
    /// <param name="method">The HTTP verb of the operation to be documented</param>
    /// <returns>The documentation added by the individual concerns, starting with the innermost one</returns>
    public IReadOnlyList<OperationDocumentation> GetDocumentation(RequestMethod method)
    {
        var result = new List<OperationDocumentation>();

        for (var current = this; current.Concern != null; current = current.Outer!)
        {
            var operation = new OperationDocumentation(method);

            current.Concern.AddDocumentation(operation);

            if (operation.Entries.Count > 0)
            {
                result.Add(operation);
            }
        }

        return result;
    }

    #endregion

}
