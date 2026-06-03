using System.Text;
using GenHTTP.Api.Protocol;
using GenHTTP.Modules.Conversion.Formatters;

namespace GenHTTP.Testing.Acceptance.Modules.Conversion.Formatters;

[TestClass]
public sealed class GuidFormatterTests
{
    private readonly GuidFormatter _formatter = new();

    private static readonly Guid TestGuid = new("a1b2c3d4-e5f6-7890-abcd-ef1234567890");

    private static ByteString Bytes(string s) => new(s);

    [TestMethod]
    public void HandlesOnlyGuid()
    {
        Assert.IsTrue(_formatter.CanHandle(typeof(Guid)));
        Assert.IsFalse(_formatter.CanHandle(typeof(string)));
        Assert.IsFalse(_formatter.CanHandle(typeof(int)));
    }

    [TestMethod]
    public void ReadsValidGuid() => Assert.AreEqual(TestGuid, _formatter.Read(Bytes(TestGuid.ToString()), typeof(Guid)));

    [TestMethod]
    public void ThrowsForInvalidInput() => Assert.ThrowsExactly<ArgumentException>(() => _formatter.Read(Bytes("not-a-guid"), typeof(Guid)));

    [TestMethod]
    public void WritesGuidAsString() => Assert.AreEqual(TestGuid.ToString(), _formatter.Write(TestGuid, typeof(Guid)));
    
}
