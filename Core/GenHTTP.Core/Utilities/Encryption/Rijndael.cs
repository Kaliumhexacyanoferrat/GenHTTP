using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Security.Cryptography;

namespace GenHTTP.Utilities.Encryption {
  
  /// <summary>
  /// Allows you to encrypt text using the Rijndael algorithm.
  /// </summary>
  /// <remarks>
  /// Adapted version of this snippet: http://dotnet-snippets.de/dns/encrypt-and-decrypt-strings-SID205.aspx
  /// This class will return UTF8 strings instead of unicode.
  /// </remarks>
  public class RijndaelEncryption : IEncryption {
    private string _Key;
    private byte[] _IV;

    /// <summary>
    /// Create a new encryption handler.
    /// </summary>
    /// <param name="key">The key to encrypt and decrypt with</param>
    public RijndaelEncryption(string key) {
      _Key = key;
      _IV = Encoding.UTF8.GetBytes("a/62)!HR)§r´b)FBDSF#ASF!");
    }

    /// <summary>
    /// Encrypt a text.
    /// </summary>
    /// <param name="text">The text to encrypt</param>
    /// <returns>The encrypted text</returns>
    public string Encrypt(string text) {
      byte[] clearBytes = System.Text.Encoding.Unicode.GetBytes(text);
      PasswordDeriveBytes pdb = new PasswordDeriveBytes(_Key, _IV);
      byte[] encryptedData = EncryptBytes(clearBytes, pdb.GetBytes(32), pdb.GetBytes(16));
      return Convert.ToBase64String(encryptedData);
    }

    /// <summary>
    /// Decrypt a cipher.
    /// </summary>
    /// <param name="text">The cipher to decrypt</param>
    /// <returns>The clear text</returns>
    public string Decrypt(string text) {
      byte[] cipherBytes = Convert.FromBase64String(text);
      PasswordDeriveBytes pdb = new PasswordDeriveBytes(_Key, _IV);
      byte[] decryptedData = DecryptBytes(cipherBytes, pdb.GetBytes(32), pdb.GetBytes(16));
      Encoding utf8 = Encoding.UTF8;
      Encoding unicode = Encoding.Unicode;
      return utf8.GetString(Encoding.Convert(unicode, utf8, decryptedData));
    }

    private static byte[] EncryptBytes(byte[] clearText, byte[] key, byte[] iv) {
      MemoryStream ms = new MemoryStream();
      Rijndael alg = Rijndael.Create();
      alg.Key = key;
      alg.IV = iv;
      CryptoStream cs = new CryptoStream(ms, alg.CreateEncryptor(), CryptoStreamMode.Write);
      cs.Write(clearText, 0, clearText.Length);
      cs.Close();
      byte[] encryptedData = ms.ToArray();
      return encryptedData;
    }

    private static byte[] DecryptBytes(byte[] cipherData, byte[] key, byte[] iv) {
      MemoryStream ms = new MemoryStream();
      Rijndael alg = Rijndael.Create();
      alg.Key = key;
      alg.IV = iv;
      CryptoStream cs = new CryptoStream(ms, alg.CreateDecryptor(), CryptoStreamMode.Write);
      cs.Write(cipherData, 0, cipherData.Length);
      cs.Close();
      byte[] decryptedData = ms.ToArray();
      return decryptedData;
    }

  }

}
