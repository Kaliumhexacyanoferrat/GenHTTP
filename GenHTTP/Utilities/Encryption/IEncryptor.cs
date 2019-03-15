using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GenHTTP.Utilities.Encryption {
  
  /// <summary>
  /// This interface defines, which methods need to be provided by an encryptor.
  /// </summary>
  public interface IEncryption {

    /// <summary>
    /// Encrypt a string.
    /// </summary>
    /// <param name="text">The string to encrypt</param>
    /// <returns>The encrypted string</returns>
    string Encrypt(string text);

    /// <summary>
    /// Decrypt a string.
    /// </summary>
    /// <param name="text">The string to decrypt</param>
    /// <returns>The decrypted string</returns>
    string Decrypt(string text);

  }

}
