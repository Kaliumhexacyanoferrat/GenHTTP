using ioxide;

namespace GenHTTP.Engine.Ioxide;

/// <summary>
/// Per-reactor access seam. Each ioxide reactor runs on its own thread; the engine binds the
/// current <see cref="Reactor" /> into a <c>[ThreadStatic]</c> slot when that thread starts, so
/// handler code (which runs on the reactor thread) can resolve per-reactor, ring-native services
/// that were registered through the <c>onReactorStart</c> host hook — for example
/// <c>IoxideReactor.Current.GetService&lt;PgPool&gt;()</c>.
/// </summary>
/// <remarks>
/// Only valid on a reactor thread (i.e. inside request handling). It relies on awaited
/// continuations resuming inline on the same reactor thread — the affinity the
/// <c>ConnectionDriver</c> thread-hop diagnostic verifies. Under a work-stealing scheduler the
/// slot could point at the wrong reactor.
/// </remarks>
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
