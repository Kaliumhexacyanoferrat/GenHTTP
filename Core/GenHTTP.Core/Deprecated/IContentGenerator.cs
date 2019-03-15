using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GenHTTP {

  /// <summary>
  /// This interface can be used to seperate content generation from project handling.
  /// </summary>
  /// <remarks>
  /// In your project class, you could manage a list with various content generator implementations.
  /// <code>
  /// private List&#60;IContentGenerator&#62; _Generators;
  /// // ...
  /// public void HandleRequest(HttpRequest request, HttpResponse response, ClientHandler handler) {
  ///   bool handled = false;
  ///   foreach (IContentGenerator generator in _Generators) {
  ///     if (generator.CanHandleRequest(request)) {
  ///       try {
  ///         generator.HandleRequest(request, response, handler);
  ///         handled = true;
  ///       }
  ///       catch {
  ///         // send 500
  ///       }
  ///     }
  ///   }
  ///   if (!handled) // send 404
  /// }
  /// </code>
  /// </remarks>
  public interface IContentGenerator {

    /// <summary>
    /// Specifies, whether this content generator is able to handle the given request.
    /// </summary>
    /// <param name="request">The request to check</param>
    /// <returns>true, if this content generator can handle this request</returns>
    /// <param name="tag">Additional information, useful for the content generator</param>
    bool CanHandleRequest(HttpRequest request, object tag);

    /// <summary>
    /// Handles the given request.
    /// </summary>
    /// <param name="request">The request to handle</param>
    /// <param name="response">The response to write to</param>
    /// <param name="handler">The responsible <see cref="ClientHandler" /></param>
    /// <param name="tag">Additional information, useful for the content generator</param>
    void HandleRequest(HttpRequest request, HttpResponse response, ClientHandler handler, object tag);

  }

}
