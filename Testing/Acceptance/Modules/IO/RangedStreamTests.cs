using System;
using System.IO;
using System.Text;

using GenHTTP.Modules.IO.Ranges;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GenHTTP.Testing.Acceptance.Modules.IO
{

    [TestClass]
    public class RangedStreamTests
    {

        [TestMethod]
        public void TestFullRange()
        {
            Assert.AreEqual("0123456789", GetRange(0, 9, 0, 10));
        }

        [TestMethod]
        public void TestNotAllWritten()
        {
            Assert.AreEqual("23", GetRange(0, 9, 2, 2));
        }

        [TestMethod]
        public void TestRangeExtracted()
        {
            Assert.AreEqual("3456", GetRange(3, 6, 0, 10));
        }

        [TestMethod]
        public void TestNothingToWrite()
        {
            Assert.AreEqual("", GetRange(12, 14, 0, 10));
        }

        [TestMethod]
        public void TestEndOfLargeFile()
        {
            Assert.AreEqual("12345", GetRange(10_001, 10_005, 0, 10, position: 10_000));
        }

        [TestMethod]
        public void TestSomewhereInLargeFile()
        {
            Assert.AreEqual("0123456789", GetRange(0, 10_000, 0, 10, position: 5000));
        }

        [TestMethod]
        public void TestOutOfLargeFile()
        {
            Assert.AreEqual("", GetRange(0, 10_000, 0, 10, position: 15_000));
        }

        [TestMethod]
        public void TestBasics()
        {
            using var stream = new RangedStream(new MemoryStream(), 0, 10);

            Assert.AreEqual(0, stream.Position);
            Assert.AreEqual(10, stream.Length);

            Assert.IsTrue(stream.CanWrite);

            Assert.IsFalse(stream.CanRead);
            Assert.IsFalse(stream.CanSeek);

            Assert.ThrowsException<NotSupportedException>(() => stream.Read(Array.Empty<byte>(), 0, 1));

            Assert.ThrowsException<NotSupportedException>(() => stream.Seek(0, SeekOrigin.Begin));

            Assert.ThrowsException<NotSupportedException>(() => stream.SetLength(0));
        }

        private static string GetRange(ulong start, ulong end, int offset, int count, int position = 0)
        {
            using var target = new MemoryStream();

            using var stream = new RangedStream(target, start, end);

            stream.Position = position;
                
            stream.Write(Encoding.ASCII.GetBytes("0123456789"), offset, count);

            return Encoding.ASCII.GetString(target.ToArray());
        }

    }

}
