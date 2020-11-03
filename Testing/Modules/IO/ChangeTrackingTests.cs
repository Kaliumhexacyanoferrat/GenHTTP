using System.IO;

using Xunit;

using GenHTTP.Modules.IO;

namespace GenHTTP.Testing.Acceptance.Modules.IO
{

    public class ChangeTrackingTests
    {

        [Fact]
        public void TestChanges()
        {
            var file = Path.GetTempFileName();

            try
            {
                File.WriteAllText(file, "One");

                var resource = Resource.FromFile(file)
                                       .Build()
                                       .Track();

                using (var _ = resource.GetContent()) { }

                Assert.False(resource.Changed);

                File.WriteAllText(file, "Two");

                Assert.True(resource.Changed);
                Assert.True(resource.Changed);
            }
            finally
            {
                try { File.Delete(file); } catch { /* nop */ }
            }
        }

        [Fact]
        public void TestBuildWithTracking()
        {
            Assert.True(Resource.FromAssembly("File.txt").BuildWithTracking().Changed);
        }

    }

}
