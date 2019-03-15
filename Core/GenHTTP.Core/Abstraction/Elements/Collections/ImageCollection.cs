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

using GenHTTP.Abstraction.Elements.Containers;

namespace GenHTTP.Abstraction.Elements.Collections {
  
  /// <summary>
  /// Allows you to add images to a container.
  /// </summary>
  public class ImageCollection : IImageContainer {
    private AddElement _Delegate;

    #region Constructors

    /// <summary>
    /// Create a new image collection.
    /// </summary>
    /// <param name="d">The method used to add elements to the container</param>
    public ImageCollection(AddElement d) {
      _Delegate = d;
    }

    #endregion

    #region IImageContainer Members

    /// <summary>
    /// Add an image to this element.
    /// </summary>
    /// <param name="source">The URL of the image</param>
    /// <param name="alternativeDescription">An alternative description (required)</param>
    /// <returns>The created object</returns>
    public Image AddImage(string source, string alternativeDescription) {
      Image img = new Image(source, alternativeDescription);
      _Delegate(img);
      return img;
    }

    /// <summary>
    /// Add an image to this element.
    /// </summary>
    /// <param name="source">The URL of the image</param>
    /// <param name="alternativeDescription">An alternative description (required)</param>
    /// <param name="longDescription">The URL of a document containing additional information about this image</param>
    /// <returns>The created object</returns>
    public Image AddImage(string source, string alternativeDescription, string longDescription) {
      Image img = new Image(source, alternativeDescription, longDescription);
      _Delegate(img);
      return img;
    }

    /// <summary>
    /// Add an image to this element.
    /// </summary>
    /// <param name="source">The URL of the image</param>
    /// <param name="alternativeDescription">An alternative description (required)</param>
    /// <param name="longDescription">The URL of a document containing additional information about this image</param>
    /// <param name="mapSource">The map to use for this image</param>
    /// <returns>The created object</returns>
    public Image AddImage(string source, string alternativeDescription, string longDescription, string mapSource) {
      Image img = new Image(source, alternativeDescription, longDescription, mapSource);
      _Delegate(img);
      return img;
    }

    /// <summary>
    /// Add an image to this element.
    /// </summary>
    /// <param name="source">The URL of the image</param>
    /// <param name="alternativeDescription">An alternative description (required)</param>
    /// <param name="isMap">Specifies, whether this image should be used as a map</param>
    /// <returns>The created object</returns>
    public Image AddImage(string source, string alternativeDescription, bool isMap) {
      Image img = new Image(source, alternativeDescription, isMap);
      _Delegate(img);
      return img;
    }

    /// <summary>
    /// Add an image to this element.
    /// </summary>
    /// <param name="source">The URL of the image</param>
    /// <param name="alternativeDescription">An alternative description (required)</param>
    /// <param name="longDescription">The URL of a document containing additional information about this image</param>
    /// <param name="isMap">Specifies, whether this image should be used as a map</param>
    /// <returns>The created object</returns>
    public Image AddImage(string source, string alternativeDescription, string longDescription, bool isMap) {
      Image img = new Image(source, alternativeDescription, longDescription, isMap);
      _Delegate(img);
      return img;
    }

    #endregion

  }

}
