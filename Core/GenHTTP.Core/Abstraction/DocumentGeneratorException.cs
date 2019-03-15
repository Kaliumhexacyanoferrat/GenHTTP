/*

Updated: 2009/10/14

2009/10/14  Andreas Nägeli        Initial version of this file.


LICENSE: This file is part of the GenHTTP webserver.

GenHTTP is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation, either version 3 of the License, or
any later version.

*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GenHTTP.Abstraction {
  
  /// <summary>
  /// An exception of this type will be thrown by the method
  /// <see cref="Document.Serialize()" /> if the generation of
  /// the document fails.
  /// </summary>
  public class DocumentGeneratorException : Exception {
  
    /// <summary>
    /// Create a new DocumentGeneratorException.
    /// </summary>
    /// <param name="message">The message of this exception</param>
    public DocumentGeneratorException(string message) : base("Failed to generate document: " + message) {

    }

  }

}
