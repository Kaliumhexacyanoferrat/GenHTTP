using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GenHTTP {

  /// <summary>
  /// Any class, implementing this interface can support the server by
  /// generating content for some standard cases.
  /// </summary>
  public interface IServerHelper {

    /// <summary>
    /// This method will get called by the server to initialize the helper.
    /// </summary>
    /// <param name="server">The server this helper should support</param>
    /// <remarks>
    /// The server is not fully initialized when it calls this method (there will be
    /// no projects and so on).
    /// </remarks>
    void Init(Server server);

    /// <summary>
    /// This method will be called by the server, whenever the server index is requested.
    /// </summary>
    /// <param name="request">The request sent by the client</param>
    /// <param name="response">The response to write to</param>
    void GenerateIndex(HttpRequest request, HttpResponse response);

    /// <summary>
    /// This method will be called by the server, whenever a non-existing page is requested
    /// by the client.
    /// </summary>
    /// <param name="request">The request sent by the client</param>
    /// <param name="response">The response to write to</param>
    /// <remarks>
    /// This method will not be called for non existing pages within a project. Such
    /// requests need to be handled by the project itself.
    /// </remarks>
    void GenerateNotFound(HttpRequest request, HttpResponse response);

    /// <summary>
    /// Whenever an exception occurs within the IProject.Run method,
    /// this method will be called.
    /// </summary>
    /// <param name="request">The request sent by the client</param>
    /// <param name="response">The response to write to</param>
    /// <param name="exception">The exception which occurred</param>
    void GenerateServerError(HttpRequest request, HttpResponse response, Exception exception);

    /// <summary>
    /// Whenever a request is not handled by the responsible project, this method
    /// will be called.
    /// </summary>
    /// <param name="request">The request sent by the client</param>
    /// <param name="response">The response to write to</param>
    void GenerateNoContent(HttpRequest request, HttpResponse response);

    /// <summary>
    /// This method will be called whenever the client sends a malformed request. 
    /// </summary>
    /// <param name="response">The response to write to</param>
    void GenerateBadRequest(HttpResponse response);

    /// <summary>
    /// Any request, which cannot be assigned to a project, will trigger this method.
    /// </summary>
    /// <param name="request">The request sent by the client</param>
    /// <param name="response">The response to write to</param>
    /// <returns>true, if the request was handled by this method</returns>
    bool GenerateOther(HttpRequest request, HttpResponse response);

  }

}
