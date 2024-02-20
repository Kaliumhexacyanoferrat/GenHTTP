using GenHTTP.Modules.DirectoryBrowsing.Provider;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GenHTTP.Testing.Acceptance.Modules.DirectoryBrowsing
{

    [TestClass]
    public sealed class FormatterTest
    {

        [TestMethod]
        public void TestFormatting()
        {
            Assert.AreEqual("512 Bytes", FileSizeFormatter.Format(512));

            Assert.AreEqual("2.78 KB", FileSizeFormatter.Format(2842));

            Assert.AreEqual("2.78 MB", FileSizeFormatter.Format(2842 * 1024));

            Assert.AreEqual("2.78 GB", FileSizeFormatter.Format(2842L * 1024 * 1024));

            Assert.AreEqual("2.78 TB", FileSizeFormatter.Format(2842L * 1024 * 1024 * 1024));
        }

    }

}
