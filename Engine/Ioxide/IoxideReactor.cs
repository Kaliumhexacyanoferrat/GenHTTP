using ioxide;

namespace GenHTTP.Engine.Ioxide;

/// <summary>
/// Per-reactor access seam: resolves the ring-native services registered through the
/// <c>onReactorStart</c> host hook, e.g. <c>IoxideReactor.Current.GetService&lt;PgPool&gt;()</c>.
/// Only valid on a reactor thread, since it relies on continuations resuming inline on one.
/// </summary>
public static class IoxideReactor
{
    [ThreadStatic]
    private static Reactor? _current;

    /// <summary>
    /// The reactor servicing the current thread. Throws if accessed off a reactor thread.
    /// </summary>
    public static Reactor Current => _current
        ?? throw new InvalidOperationException("IoxideReactor.Current is only available on a reactor thread (inside request handling).");

    internal static void Bind(Reactor reactor) => _current = reactor;
}
