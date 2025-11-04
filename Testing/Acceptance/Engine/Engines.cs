namespace GenHTTP.Testing.Acceptance.Engine;

public class Engines
{

    public static bool KestrelEnabled() => Environment.GetEnvironmentVariable("TEST_ENGINE") == null;

}
