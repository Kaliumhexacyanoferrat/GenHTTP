using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GenHTTP.Api.Abstraction
{

    /// <summary>
    /// An exception of this type will be thrown by the method
    /// <see cref="Document.Serialize()" /> if the generation of
    /// the document fails.
    /// </summary>
    public class DocumentGeneratorException : Exception
    {

        /// <summary>
        /// Create a new DocumentGeneratorException.
        /// </summary>
        /// <param name="message">The message of this exception</param>
        public DocumentGeneratorException(string message) : base("Failed to generate document: " + message)
        {

        }

    }

}
