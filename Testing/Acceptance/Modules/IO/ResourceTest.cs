using System;
using System.IO;
using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using GenHTTP.Api.Content.IO;
using GenHTTP.Api.Protocol;

using GenHTTP.Modules.IO;
using GenHTTP.Modules.Layouting;

using GenHTTP.Testing.Acceptance.Utilities;

namespace GenHTTP.Testing.Acceptance.Modules.IO
{

    [TestClass]
    public sealed class ResourceTest
    {

        [TestMethod]
        public async Task TestStringResource()
        {
            var resource = Resource.FromString("Hello World")
                                   .Build();

            Assert.AreEqual(ContentType.TextPlain, resource.ContentType?.KnownType);

            using var content = await resource.GetContentAsync();

            Assert.AreEqual(11, content.Length);

            Assert.AreEqual((ulong)11, resource.Length!);

            Assert.IsNull(resource.Modified);
            Assert.IsNull(resource.Name);
        }

        [TestMethod]
        public async Task TestFileResource()
        {
            var file = Path.GetTempFileName();

            try
            {
                await FileUtil.WriteTextAsync(file, "Hello World");

                var resource = Resource.FromFile(file)
                                       .Build();

                using var content = await resource.GetContentAsync();

                Assert.AreEqual(11, content.Length);

                Assert.AreEqual((ulong)11, resource.Length!);

                Assert.IsNotNull(resource.Modified);
                Assert.IsNotNull(resource.Name);
            }
            finally
            {
                try { File.Delete(file); } catch { /* nop */ }
            }
        }

        [TestMethod]
        public async Task TestAssemblyResource()
        {
            var resource = Resource.FromAssembly("File.txt")
                                   .Build();

            Assert.AreEqual(ContentType.TextPlain, resource.ContentType?.KnownType);

            using var content = await resource.GetContentAsync();

            Assert.AreEqual(16, content.Length);

            Assert.AreEqual((ulong)16, resource.Length!);

            Assert.IsNotNull(resource.Modified);
        }

        [TestMethod]
        public async Task TestAssemblyResourceRouting()
        {
            var layout = Layout.Create()
                               .Add("1", Content.From(Resource.FromAssembly("File.txt")))
                               .Add("2", Content.From(Resource.FromAssembly("OtherFile.txt")));

            using var runner = TestHost.Run(layout);

            using var f1 = await runner.GetResponseAsync("/1");
            Assert.AreEqual("This is text!", await f1.GetContentAsync());

            using var f2 = await runner.GetResponseAsync("/2");
            Assert.AreEqual("This is other text!", await f2.GetContentAsync());
        }

        [TestMethod]
        public void TestStringMetaData()
        {
            TestMetaData(Resource.FromString("Hello World"));
        }

        [TestMethod]
        public void TestEmbeddedMetaData()
        {
            TestMetaData(Resource.FromAssembly("File.txt"));
        }

        [TestMethod]
        public void TestFileMetaData()
        {
            var file = Path.GetTempFileName();

            FileUtil.WriteText(file, "blubb");

            try
            {
                TestMetaData(Resource.FromFile(file), false);
            }
            finally
            {
                try { File.Delete(file); } catch { /* nop */ }
            }
        }

        private static void TestMetaData<T>(IResourceBuilder<T> builder, bool modified = true) where T : IResourceBuilder<T>
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

            Assert.AreEqual("MyFile.txt", resource.Name);
            Assert.AreEqual(ContentType.VideoH264, resource.ContentType?.KnownType);

            if (modified)
            {
                Assert.AreEqual(now, resource.Modified);
            }
        }

        [TestMethod]
        public void TestNonExistentFile()
        {
            Assert.ThrowsException<FileNotFoundException>(() => Resource.FromFile("blubb.txt").Build());
        }

    }

}
