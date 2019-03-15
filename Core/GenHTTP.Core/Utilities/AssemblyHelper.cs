using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;

namespace GenHTTP.Utilities {
  
  /// <summary>
  /// The assembly helpers allows you to instantiate classes of a given type during runtime.
  /// </summary>
  public class AssemblyHelper {

    /// <summary>
    /// Instantiate a class in the assembly implementing the given interface.
    /// </summary>
    /// <typeparam name="T">The interface to search for</typeparam>
    /// <param name="assemblyFile">The assembly to search in</param>
    /// <returns>An instance of the given type or null, if no matching class could be found</returns>
    /// <remarks>
    /// This method will return an instance of the first matching class.
    /// </remarks>
    public static T Instantiate<T>(string assemblyFile) {
      try {
        Assembly targetAssembly = Assembly.LoadFile(assemblyFile);
        foreach (Type t in targetAssembly.GetTypes()) { // search for classes implementing the given interface
          if (t.IsPublic && t.IsClass) {
            Type[] test = t.FindInterfaces(new TypeFilter(CheckInterface), typeof(T));
            if (test.Length > 0) { // this class implements our interface
              object o = targetAssembly.CreateInstance(t.Namespace + "." + t.Name);
              return (T)o;
            }
          }
        }
      }
      catch { }
      return default(T);
    }

    /// <summary>
    /// Create an object of the given class from the given assembly.
    /// </summary>
    /// <typeparam name="T">The type to instantiate</typeparam>
    /// <param name="assemblyFile">The file to search in</param>
    /// <param name="typeName">The type to search for</param>
    /// <returns>The requested instance or null, if no class with this type could be found</returns>
    public static T Instantiate<T>(string assemblyFile, string typeName) {
      try {
        Assembly targetAssembly = Assembly.LoadFile(assemblyFile);
        return (T)targetAssembly.CreateInstance(typeName);
      }
      catch {}
      return default(T);
    }

    private static bool CheckInterface(Type t, object o) {
      Type filter = (Type)o;
      return (t.Name == filter.Name && t.Namespace == filter.Namespace);
    }

  }

}
