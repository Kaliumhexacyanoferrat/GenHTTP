/*

Updated: 2009/10/30

2009/10/30  Andreas Nägeli        Initial version of this file.


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

namespace GenHTTP.Caching {
  
  /// <summary>
  /// Defines the signature of a method, used to determine, whether
  /// the cache should send a cached response to the client or not.
  /// </summary>
  /// <param name="request">The request to handle</param>
  /// <param name="info">Information about the client's status</param>
  /// <returns>true, if the cache should send a cached response</returns>
  public delegate bool CheckApplicability(HttpRequest request, AuthorizationInfo info);

}
