using ioxide;

namespace GenHTTP.Engine.Ioxide;

/// <summary>The reactor the calling thread is running, for host code that needs to reach it.</summary>
public static class IoxideReactor
{
    [ThreadStatic]
    private static Reactor? _current;

    public static Reactor Current => _current
        ?? throw new InvalidOperationException("IoxideReactor.Current is only available on a reactor thread (inside request handling).");

    // Pins a reactor to its own thread, which is how Current finds it later.
    internal static void Bind(Reactor reactor) => _current = reactor;
}
