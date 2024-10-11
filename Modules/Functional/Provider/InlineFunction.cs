using System;

using GenHTTP.Modules.Reflection;

namespace GenHTTP.Modules.Functional.Provider;

public record InlineFunction(string? Path, IMethodConfiguration Configuration, Delegate Delegate);
