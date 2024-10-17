using GenHTTP.Api.Protocol;

namespace GenHTTP.Modules.Reflection;

/// <summary>
/// Configures the method provider which invokes functionality
/// provided via reflection.
/// </summary>
public interface IMethodConfiguration
{

    /// <summary>
    /// The HTTP verbs which are supported by this method.
    /// </summary>
    public HashSet<FlexibleRequestMethod> SupportedMethods { get; }
}
