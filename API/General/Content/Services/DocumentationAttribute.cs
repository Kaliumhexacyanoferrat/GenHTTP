namespace GenHTTP.Api.Content.Services;

/// <summary>
/// Base class of the attributes that document an API, so it can be
/// presented to its consumers (e.g. in a generated OpenAPI specification).
/// </summary>
/// <remarks>
/// Besides being placed on services and their methods, instances of these
/// attributes can also be created at runtime by concerns to document the
/// functionality they add to the content they wrap (see <see cref="IDocumentedConcern" />).
/// </remarks>
public abstract class DocumentationAttribute : Attribute;
