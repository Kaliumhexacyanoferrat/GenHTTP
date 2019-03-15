using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GenHTTP.Api.Compilation
{

    /// <summary>
    /// Defines the methods of a snippet.
    /// </summary>
    public interface ISnippet
    {

        /// <summary>
        /// The content length of the snippet.
        /// </summary>
        long ContentLength { get; }

        /// <summary>
        /// Generate the output for this snippet.
        /// </summary>
        /// <returns>The content of this snippet</returns>
        byte[] ToByteArray();

        /// <summary>
        /// Generate the ouput for this snippet.
        /// </summary>
        /// <returns>The content of this snippet</returns>
        string ToString();

    }

}
