/*

Updated: 2009/10/28

2009/10/28  Andreas Nägeli        Initial version of this file.


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

using GenHTTP.SessionManagement;

namespace GenHTTP.Content {

  /// <summary>
  /// Describes methods which must be implemented by content
  /// providing classes.
  /// </summary>
  public interface IContentProvider {

    /// <summary>
    /// Specifies, whether this content provider needs a logged in user.
    /// </summary>
    bool RequiresLogin { get; }

    /// <summary>
    /// Check, whether the content provider is responsible for a given request.
    /// </summary>
    /// <param name="request">The request to check the resposibility for</param>
    /// <param name="info">Information about the client's session</param>
    /// <returns>true, if the content provider can handle this request</returns>
    bool IsResponsible(HttpRequest request, AuthorizationInfo info);

    /// <summary>
    /// Provides content to the client.
    /// </summary>
    /// <param name="request">The request to handle</param>
    /// <param name="response">The response to write to</param>
    /// <param name="info">Information about the session of the client</param>
    void HandleRequest(HttpRequest request, HttpResponse response, AuthorizationInfo info);

  }

}
