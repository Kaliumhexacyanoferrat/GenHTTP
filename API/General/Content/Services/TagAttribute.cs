namespace GenHTTP.Api.Content.Services;

/// <summary>
/// Specifies the group the operations of a service are listed under
/// when presented to the consumers of the API.
/// </summary>
/// <remarks>
/// If placed on a service class, all operations of the service will be
/// grouped under the given name instead of the name of the class. If placed
/// on a method, the operation will be listed under the given name regardless
/// of the service it is declared in.
/// </remarks>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Interface | AttributeTargets.Method)]
public sealed class TagAttribute : DocumentationAttribute
{

    #region Get-/Setters

    /// <summary>
    /// The name of the group the operations are listed under.
    /// </summary>
    public string Name { get; }

    #endregion

    #region Initialization

    /// <summary>
    /// Lists the annotated operations under the given name.
    /// </summary>
    /// <param name="name">The name of the group the operations are listed under</param>
    public TagAttribute(string name)
    {
        Name = name;
    }

    #endregion

}
