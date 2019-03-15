using GenHTTP.Api.Abstraction;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GenHTTP.Api.Compilation
{

    /// <summary>
    /// Stores the static content of a template.
    /// </summary>
    public interface ITemplateBase
    {

        /// <summary>
        /// Retrieve a static part.
        /// </summary>
        /// <param name="nr">The number of the part to retrive</param>
        /// <returns>The requested part</returns>
        byte[] this[int nr] { get; }

        /// <summary>
        /// The encoding used for this template.
        /// </summary>
        Encoding Encoding { get; }

        /// <summary>
        /// The content length of the static content.
        /// </summary>
        long ContentLength { get; }

        /// <summary>
        /// The type of the document.
        /// </summary>
        DocumentType Type { get; }

    }

}
