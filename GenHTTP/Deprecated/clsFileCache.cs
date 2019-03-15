using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace GenHTTP {
  
  /// <summary>
  /// Caches files for projects.
  /// </summary>
  [Serializable]
  public class FileCache : MarshalByRefObject {
    private Dictionary<string, byte[]> _Cache;
    private int _Max;

    /// <summary>
    /// Create a new file cache.
    /// </summary>
    /// <param name="max">The maximum file size in bytes</param>
    public FileCache(int max) {
      _Max = max;
      _Cache = new Dictionary<string, byte[]>();
    }

    /// <summary>
    /// Check whether the given file is available in the cache or not.
    /// </summary>
    /// <param name="filename">The file name to check</param>
    /// <returns>True, if the cache contains the given file</returns>
    public bool Hit(string filename) {
      return _Cache.ContainsKey(filename);
    }

    /// <summary>
    /// Retrieve the content of a cached file.
    /// </summary>
    /// <param name="filename">The file name to retrieve the content to</param>
    /// <returns>The requested file or null, if it is not cached</returns>
    public byte[] Content(string filename) {
      if (Hit(filename)) return _Cache[filename];
      return null;
    }

    /// <summary>
    /// Try to load a file into the file cache.
    /// </summary>
    /// <param name="filename">The path of the file to cache</param>
    /// <returns>True if the file has been cached successfully</returns>
    public bool Load(string filename) {
      try {
        if (Hit(filename)) return true;
        BinaryReader r = new BinaryReader(File.Open(filename, FileMode.Open));
        if (r.BaseStream.Length > _Max) {
          r.Close();
          return false;
        }
        byte[] buffer = new byte[r.BaseStream.Length];
        r.Read(buffer, 0, buffer.Length);
        r.Close();
        _Cache.Add(filename, buffer);
        return true;
      }
      catch {
        return false;
      }
    }

  }

}
