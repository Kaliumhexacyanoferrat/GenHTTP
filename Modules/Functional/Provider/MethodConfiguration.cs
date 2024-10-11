using GenHTTP.Api.Protocol;
using GenHTTP.Modules.Reflection;

namespace GenHTTP.Modules.Functional.Provider;

public record MethodConfiguration(HashSet<FlexibleRequestMethod> SupportedMethods, bool IgnoreContent, Type? ContentHints) : IMethodConfiguration;
