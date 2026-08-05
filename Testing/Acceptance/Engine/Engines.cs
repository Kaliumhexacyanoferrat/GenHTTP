namespace GenHTTP.Testing.Acceptance.Engine;

public static class Engines
{

    public static bool KestrelEnabled()
    {
        var engine = Environment.GetEnvironmentVariable("TEST_ENGINE");
        return (engine == null || engine.Equals("Kestrel", StringComparison.OrdinalIgnoreCase));
    }

    public static bool IoxideEnabled()
    {
#if NET11_0_OR_GREATER
        var engine = Environment.GetEnvironmentVariable("TEST_ENGINE");
        return OperatingSystem.IsLinux() && (engine == null || engine.Equals("Ioxide", StringComparison.OrdinalIgnoreCase));
#else
        return false;
#endif
    }
    
}
