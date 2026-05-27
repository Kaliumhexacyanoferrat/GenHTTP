// Polyfill required when targeting netstandard2.0 with C# 9+ features
// (records, init-only setters). The compiler emits references to this type
// but it only exists in .NET 5+.
// See: https://github.com/dotnet/roslyn/issues/45510

using System.ComponentModel;

namespace System.Runtime.CompilerServices;

/// <summary>
/// Reserved to be used by the compiler for tracking metadata.
/// This class should not be used by developers in source code.
/// </summary>
[EditorBrowsable(EditorBrowsableState.Never)]
internal static class IsExternalInit { }
