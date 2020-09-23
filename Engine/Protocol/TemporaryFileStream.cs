using System;
using System.IO;

namespace GenHTTP.Engine.Protocol
{

    internal class TemporaryFileStream : FileStream
    {

        #region Get-/Setters

        internal string TemporaryFile { get; }

        #endregion

        #region Initialization

        private TemporaryFileStream(string file) : base(file, FileMode.CreateNew, FileAccess.ReadWrite)
        {
            TemporaryFile = file;
        }

        internal static Stream Create()
        {
            var fileName = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString() + ".genhttp.tmp");
            return new TemporaryFileStream(fileName);
        }

        #endregion

        #region Lifecycle

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            if (disposing)
            {
                if (File.Exists(TemporaryFile))
                {
                    File.Delete(TemporaryFile);
                }
            }
        }

        #endregion

    }

}
