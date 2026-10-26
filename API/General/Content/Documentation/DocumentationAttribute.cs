namespace GenHTTP.Api.Content.Documentation;

/// <summary>
/// Base class of the attributes that document an API, so it can be
/// presented to its consumers (e.g. in a generated OpenAPI specification).
/// </summary>
/// <remarks>
/// Concerns document the functionality they add to the content they wrap
/// via <see cref="OperationDocumentation" />, which creates instances of these
/// attributes at runtime (see <see cref="IDocumentedConcern" />).
/// </remarks>
public abstract class DocumentationAttribute : Attribute;
