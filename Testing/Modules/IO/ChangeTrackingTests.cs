using System.IO;
using System.Threading.Tasks;

using Xunit;

using GenHTTP.Modules.IO;

namespace GenHTTP.Testing.Acceptance.Modules.IO
{

    public class ChangeTrackingTests
    {

        [Fact]
        public async ValueTask TestChanges()
        {
            var file = Path.GetTempFileName();

            try
            {
                await File.WriteAllTextAsync(file, "One");

                var resource = Resource.FromFile(file)
                                       .Build()
                                       .Track();

                using (var _ = await resource.GetContentAsync()) { }

                Assert.False(await resource.HasChanged());

                // modification timestamp is in seconds on unix, so we need another length
                await File.WriteAllTextAsync(file, "Three");

                Assert.True(await resource.HasChanged());
            }
            finally
            {
                try { File.Delete(file); } catch { /* nop */ }
            }
        }

        [Fact]
        public async ValueTask TestBuildWithTracking()
        {
            Assert.True(await Resource.FromAssembly("File.txt").BuildWithTracking().HasChanged());
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
