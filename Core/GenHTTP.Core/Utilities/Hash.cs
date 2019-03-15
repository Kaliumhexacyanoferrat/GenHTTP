using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GenHTTP.Utilities {

  /// <summary>
  /// Helper class to hash strings.
  /// </summary>
  public static class Hash {

    /// <summary>
    /// Generate a hash sum of a string.
    /// </summary>
    /// <param name="str">The string to generate a hash to</param>
    /// <returns>The requested hash</returns>
    /// <remarks>
    /// The algorithm used here is not very strong. For hashing passwords or something like this
    /// you should use a safe algorithm (SHA-2 or something like that).
    /// </remarks>
    public static string HashString(string str) {
      long hash = 3203431780337;
      for (int i = 0; i < str.Length; i++) {
        hash *= (i+1) * Convert.ToUInt32(str[i]);
        if (hash == 0) hash = 1;
      }
      if (hash < 0) hash = -hash;
      return hash.ToString("X");
    }

    /// <summary>
    /// Creates a salted hash.
    /// </summary>
    /// <param name="str">The string to hash</param>
    /// <returns>The salted hash</returns>
    public static string SaltedHashString(string str) {
      return HashString(")\r(HR§973b=\"=(§" + str + "`8)(\"3\n9^°");
    }

    /// <summary>
    /// Creates a salted hash.
    /// </summary>
    /// <param name="str">The string to hash</param>
    /// <param name="preSalt">The salt to add at the beginning of the string</param>
    /// <param name="postSalt">The salt to add at the end of the string</param>
    /// <returns>The salted hash</returns>
    public static string SaltedHashString(string str, string preSalt, string postSalt) {
      return HashString(preSalt + str + postSalt);
    }

  }

}
