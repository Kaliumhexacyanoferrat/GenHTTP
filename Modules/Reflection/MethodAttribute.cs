using GenHTTP.Api.Protocol;

namespace GenHTTP.Modules.Reflection;

/// <summary>
/// Attribute indicating that this method can be invoked
/// via reflection.
/// </summary>
[AttributeUsage(AttributeTargets.Method)]
public class MethodAttribute : Attribute, IMethodConfiguration
{

    #region Get-/Setters

    /// <summary>
    /// The HTTP verbs which are supported by this method.
    /// </summary>
    public HashSet<FlexibleRequestMethod> SupportedMethods { get; }

    #endregion

    #region Initialization

    /// <summary>
    /// Marks the method as a invokable function.
    /// </summary>
    public MethodAttribute()
    {
        SupportedMethods = new HashSet<FlexibleRequestMethod>(2)
        {
            FlexibleRequestMethod.Get(RequestMethod.Get),
            FlexibleRequestMethod.Get(RequestMethod.Head)
        };
    }

    /// <summary>
    /// Marks the method as a invokable function for the specified HTTP verbs.
    /// </summary>
    /// <param name="methods">The HTTP verbs supported by this method</param>
    public MethodAttribute(params RequestMethod[] methods)
    {
        SupportedMethods = new HashSet<FlexibleRequestMethod>(methods.Select(FlexibleRequestMethod.Get));
    }

    /// <summary>
    /// Marks the method as a invokable function for the specified HTTP verbs.
    /// </summary>
    /// <param name="methods">The HTTP verbs supported by this method</param>
    public MethodAttribute(params FlexibleRequestMethod[] methods)
    {
        SupportedMethods = new HashSet<FlexibleRequestMethod>(methods);
    }

    #endregion

}
