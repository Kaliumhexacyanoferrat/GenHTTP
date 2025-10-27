namespace GenHTTP.Testing.Acceptance.Engine.Kestrel;

public class KestrelBaseTest
{

    protected bool CheckKestrel() => Environment.GetEnvironmentVariable("TEST_ENGINE") == null;

}
