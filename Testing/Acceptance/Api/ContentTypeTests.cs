using GenHTTP.Modules.Conversion;

namespace GenHTTP.Testing.Acceptance.Api;

[TestClass]
public class ContentTypeTests
{

    [TestMethod]
    public void TestParallelAccess()
    {
        Parallel.For(0, 10, _ =>
        {
            Serialization.Default();
        });
    }
    
}
