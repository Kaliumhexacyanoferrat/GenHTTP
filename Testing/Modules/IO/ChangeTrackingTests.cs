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

                // modification timestamp is in seconds on unix, so we need another length
                File.WriteAllText(file, "Three");

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

        [Fact]
        public void TestMetaInformation()
        {
            var resource = Resource.FromAssembly("File.txt").Build();

            var tracked = resource.Track();

            Assert.Equal(resource.Name, tracked.Name);
            Assert.Equal(resource.Length, tracked.Length);
            Assert.Equal(resource.Modified, tracked.Modified);
            Assert.Equal(resource.ContentType, tracked.ContentType);
        }

    }

}
