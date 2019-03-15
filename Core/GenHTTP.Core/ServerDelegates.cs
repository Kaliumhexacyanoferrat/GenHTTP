/*

Updated: 2009/10/29

2009/10/29  Andreas Nägeli        Initial version of this file.


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

namespace GenHTTP {

  /// <summary>
  /// Signature of a function which will be called after a request has been handled by a project.
  /// </summary>
  /// <param name="request">The request sent to the server</param>
  /// <param name="response">The response of the project</param>
  public delegate void RequestHandled(HttpRequest request, HttpResponse response);

  /// <summary>
  /// Signature of the method which will be called if the timer ticks.
  /// </summary>
  public delegate void TimerTick();

}
