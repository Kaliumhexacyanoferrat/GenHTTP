using System;
using System.IO;

using Xunit;

using GenHTTP.Api.Content.IO;
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

        [Fact]
        public void TestStringMetaData()
        {
            TestMetaData(Resource.FromString("Hello World"));
        }

        [Fact]
        public void TestEmbeddedMetaData()
        {
            TestMetaData(Resource.FromAssembly("File.txt"));
        }

        [Fact]
        public void TestFileMetaData()
        {
            var file = Path.GetTempFileName();

            File.WriteAllText(file, "blubb");

            try
            {
                TestMetaData(Resource.FromFile(file), false);
            }
            finally
            {
                try { File.Delete(file); } catch { /* nop */ }
            }
        }

        private void TestMetaData<T>(IResourceMetaDataBuilder<T> builder, bool modified = true) where T : IResourceMetaDataBuilder<T>
        {
            var now = DateTime.UtcNow;

            builder.Name("MyFile.txt")
                   .Type(ContentType.VideoH264)
                   .Type(new FlexibleContentType(ContentType.VideoH264));

            if (modified)
            {
                builder.Modified(now);
            }

            var resource = builder.Build();

            Assert.Equal("MyFile.txt", resource.Name);
            Assert.Equal(ContentType.VideoH264, resource.ContentType?.KnownType);

            if (modified)
            {
                Assert.Equal(now, resource.Modified);
            }
        }

        [Fact]
        public void TestNonExistentFile()
        {
            Assert.Throws<FileNotFoundException>(() => Resource.FromFile("blubb.txt").Build());
        }

    }

}
