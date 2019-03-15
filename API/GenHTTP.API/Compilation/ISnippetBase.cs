using GenHTTP.Api.Abstraction;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GenHTTP.Api.Compilation
{

    /// <summary>
    /// This interface defines methods for a snippet base.
    /// </summary>
    public interface ISnippetBase
    {

        /// <summary>
        /// Retrieve parts of the snippet base.
        /// </summary>
        /// <param name="nr">The number of the snippet to retrieve</param>
        /// <returns>The content of the part</returns>
        byte[] this[int nr] { get; }

        /// <summary>
        /// The encoding of the related document.
        /// </summary>
        Encoding Encoding { get; }

        /// <summary>
        /// The type of the related document.
        /// </summary>
        DocumentType Type { get; }

    }

}
