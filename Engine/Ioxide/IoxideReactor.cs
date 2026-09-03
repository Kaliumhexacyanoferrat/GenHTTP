using ioxide;

namespace GenHTTP.Engine.Ioxide;

public static class IoxideReactor
{
    [ThreadStatic]
    private static Reactor? _current;

    public static Reactor Current => _current
        ?? throw new InvalidOperationException("IoxideReactor.Current is only available on a reactor thread (inside request handling).");

    internal static void Bind(Reactor reactor) => _current = reactor;
}
