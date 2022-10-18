using System;
using System.IO;

namespace GenHTTP.Engine.Protocol
{

    /// <summary>
    /// Provides and maintains a temporary file used by the server engine
    /// to save information, such as the body of a large request.
    /// </summary>
    internal sealed class TemporaryFileStream : FileStream
    {

        #region Get-/Setters

        internal string TemporaryFile { get; }

        #endregion

        #region Initialization

        private TemporaryFileStream(string file) : base(file, FileMode.CreateNew, FileAccess.ReadWrite)
        {
            TemporaryFile = file;
        }

        /// <summary>
        /// Creates a new temporary file which can be used for data storage.
        /// </summary>
        /// <returns>The newly created file stream</returns>
        internal static Stream Create()
        {
            return new TemporaryFileStream(Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString() + ".genhttp.tmp"));
        }

        #endregion

        #region Lifecycle

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            if (disposing)
            {
                File.Delete(TemporaryFile);
            }
        }

        #endregion

    }

}
