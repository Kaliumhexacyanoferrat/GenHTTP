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

namespace GenHTTP.Abstraction.Elements.Containers {
  
  /// <summary>
  /// Describes methods which must be provided by a container
  /// containing images.
  /// </summary>
  public interface IImageContainer {

    /// <summary>
    /// Add an image to this element.
    /// </summary>
    /// <param name="source">The URL of the image</param>
    /// <param name="alternativeDescription">An alternative description (required)</param>
    /// <returns>The created object</returns>
    Image AddImage(string source, string alternativeDescription);

    /// <summary>
    /// Add an image to this element.
    /// </summary>
    /// <param name="source">The URL of the image</param>
    /// <param name="alternativeDescription">An alternative description (required)</param>
    /// <param name="longDescription">The URL of a document containing additional information about this image</param>
    /// <returns>The created object</returns>
    Image AddImage(string source, string alternativeDescription, string longDescription);

    /// <summary>
    /// Add an image to this element.
    /// </summary>
    /// <param name="source">The URL of the image</param>
    /// <param name="alternativeDescription">An alternative description (required)</param>
    /// <param name="longDescription">The URL of a document containing additional information about this image</param>
    /// <param name="mapSource">The map to use for this image</param>
    /// <returns>The created object</returns>
    Image AddImage(string source, string alternativeDescription, string longDescription, string mapSource);

    /// <summary>
    /// Add an image to this element.
    /// </summary>
    /// <param name="source">The URL of the image</param>
    /// <param name="alternativeDescription">An alternative description (required)</param>
    /// <param name="isMap">Specifies, whether this image should be used as a map</param>
    /// <returns>The created object</returns>
    Image AddImage(string source, string alternativeDescription, bool isMap);

    /// <summary>
    /// Add an image to this element.
    /// </summary>
    /// <param name="source">The URL of the image</param>
    /// <param name="alternativeDescription">An alternative description (required)</param>
    /// <param name="longDescription">The URL of a document containing additional information about this image</param>
    /// <param name="isMap">Specifies, whether this image should be used as a map</param>
    /// <returns>The created object</returns>
    Image AddImage(string source, string alternativeDescription, string longDescription, bool isMap);

  }

}
