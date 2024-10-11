using Microsoft.VisualStudio.TestTools.UnitTesting;

using GenHTTP.Modules.IO;

using GenHTTP.Testing.Acceptance.Utilities;

namespace GenHTTP.Testing.Acceptance.Modules.IO;

[TestClass]
public sealed class ChangeTrackingTests
{

    [TestMethod]
    public async Task TestChanges()
    {
            var file = Path.GetTempFileName();

            try
            {
                await FileUtil.WriteTextAsync(file, "One");

                var resource = Resource.FromFile(file)
                                       .Build()
                                       .Track();

                using (var _ = await resource.GetContentAsync()) { }

                Assert.IsFalse(await resource.HasChanged());

                // modification timestamp is in seconds on unix, so we need another length
                await FileUtil.WriteTextAsync(file, "Three");

                Assert.IsTrue(await resource.HasChanged());
            }
            finally
            {
                try { File.Delete(file); } catch { /* nop */ }
            }
        }

    [TestMethod]
    public async Task TestBuildWithTracking()
    {
            Assert.IsTrue(await Resource.FromAssembly("File.txt").BuildWithTracking().HasChanged());
        }

    [TestMethod]
    public void TestMetaInformation()
    {
            var resource = Resource.FromAssembly("File.txt").Build();

            var tracked = resource.Track();

            Assert.AreEqual(resource.Name, tracked.Name);
            Assert.AreEqual(resource.Length, tracked.Length);
            Assert.AreEqual(resource.Modified, tracked.Modified);
            Assert.AreEqual(resource.ContentType, tracked.ContentType);
        }

}
