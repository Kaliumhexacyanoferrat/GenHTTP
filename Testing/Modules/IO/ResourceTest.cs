using System.IO;

using Xunit;

using GenHTTP.Api.Protocol;
using GenHTTP.Modules.IO;

namespace GenHTTP.Testing.Acceptance.Modules.IO
{

    public class ResourceTest
    {

        [Fact]
        public void TestStringResource()
        {
            var resource = Resource.FromString("Hello World")
                                   .Build();

            Assert.Equal(ContentType.TextPlain, resource.ContentType?.KnownType);

            using var content = resource.GetContent();

            Assert.Equal(11, content.Length);

            Assert.Equal((ulong)11, resource.Length!);

            Assert.Null(resource.Modified);
            Assert.Null(resource.Name);
        }

        [Fact]
        public void TestFileResource()
        {
            var file = Path.GetTempFileName();

            try
            {
                File.WriteAllText(file, "Hello World");

                var resource = Resource.FromFile(file)
                                       .Build();

                using var content = resource.GetContent();

                Assert.Equal(11, content.Length);

                Assert.Equal((ulong)11, resource.Length!);

                Assert.NotNull(resource.Modified);
                Assert.NotNull(resource.Name);
            }
            finally
            {
                try { File.Delete(file); } catch { /* nop */ }
            }
        }

        [Fact]
        public void TestAssemblyResource()
        {
            var resource = Resource.FromAssembly("File.txt")
                                   .Build();

            Assert.Equal(ContentType.TextPlain, resource.ContentType?.KnownType);

            using var content = resource.GetContent();

            Assert.Equal(16, content.Length);

            Assert.Equal((ulong)16, resource.Length!);

            Assert.NotNull(resource.Modified);
        }

    }

}
