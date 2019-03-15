using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using GenHTTP;

namespace GenHTTP.Remoting {
  
  /// <summary>
  /// Stores a request-response pair.
  /// </summary>
  [Serializable]
  public class HandledRequest : MarshalByRefObject {
    private HttpRequest _Request;
    private HttpResponse _Response;

    /// <summary>
    /// Create a new instance of this class.
    /// </summary>
    /// <param name="request">The request handled by the server</param>
    /// <param name="response">The response sent by the server</param>
    public HandledRequest(HttpRequest request, HttpResponse response) {
      _Request = request;
      _Response = response;
    }

    #region get-/setters

    /// <summary>
    /// The request from the client.
    /// </summary>
    public HttpRequest Request { get { return _Request; } }

    /// <summary>
    /// The response sent by the server.
    /// </summary>
    public HttpResponse Response { get { return _Response; } }

    #endregion

  }

}
