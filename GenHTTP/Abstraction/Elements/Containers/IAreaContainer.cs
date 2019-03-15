/*

Updated: 2009/10/22

2009/10/22  Andreas Nägeli        Initial version of this file.


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
using System.Drawing;

namespace GenHTTP.Abstraction.Elements.Containers {
  
  /// <summary>
  /// Describes methods, which must be implemented by a container
  /// with area elements.
  /// </summary>
  public interface IAreaContainer {

    /// <summary>
    /// Add a new, empty area.
    /// </summary>
    /// <returns>The created object</returns>
    Area AddArea();

    /// <summary>
    /// Add a clickable rectangle.
    /// </summary>
    /// <param name="link">The URL of the document to link</param>
    /// <param name="alternativeDescription">The alternative description of the document</param>
    /// <param name="rectangle">The coordinates of the area</param>
    /// <returns>The created object</returns>
    Area AddArea(string link, string alternativeDescription, Rectangle rectangle);

    /// <summary>
    /// Add a clickable circle.
    /// </summary>
    /// <param name="link">The URL of the document to link</param>
    /// <param name="alternativeDescription">The alternative description of the document</param>
    /// <param name="point">The middle point of the circle</param>
    /// <param name="radius">The radius of the circle</param>
    /// <returns>The created object</returns>
    Area AddArea(string link, string alternativeDescription, Point point, ushort radius);

    /// <summary>
    /// Add a clickable polygon.
    /// </summary>
    /// <param name="link">The URL of the document to link</param>
    /// <param name="alternativeDescription">The alternative description of the document</param>
    /// <param name="points">The coordinates of the polygon</param>
    /// <returns>The created object</returns>
    Area AddArea(string link, string alternativeDescription, Point[] points);

    /// <summary>
    /// Add a clickable polygon.
    /// </summary>
    /// <param name="link">The URL of the document to link</param>
    /// <param name="alternativeDescription">The alternative description of the document</param>
    /// <param name="points">The coordinates of the polygon</param>
    /// <returns>The created object</returns>
    Area AddArea(string link, string alternativeDescription, IEnumerable<Point> points);

  }

}
