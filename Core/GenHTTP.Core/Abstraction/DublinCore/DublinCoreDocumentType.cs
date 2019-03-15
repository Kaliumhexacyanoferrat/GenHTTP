/*

Updated: 2009/10/15

2009/10/15  Andreas Nägeli        Initial version of this file.


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

namespace GenHTTP.Abstraction.DublinCore {
  
  /// <summary>
  /// The type of a document described in the
  /// scheme DCTERMS.DCMIType.
  /// </summary>
  public enum DublinCoreDocumentType {
    /// <summary>
    /// An index page containing links to other documents.
    /// </summary>
    Collection,
    /// <summary>
    /// The document presents data like a data set (e.g. using
    /// a table).
    /// </summary>
    DataSet,
    /// <summary>
    /// The document describes an event (e.g. a marriage).
    /// </summary>
    Event,
    /// <summary>
    /// The document contains an image, video or an animation.
    /// </summary>
    Image,
    /// <summary>
    /// The user can interact with this document (e.g. a chat).
    /// </summary>
    InteractiveResource,
    /// <summary>
    /// The page describes a physical object.
    /// </summary>
    /// <remarks>
    /// Should not be used for (X)HTML documents. Use Image,
    /// if the document shows the picture of a physical object.
    /// </remarks>
    PhysicalObject,
    /// <summary>
    /// The page provides a web application (e.g. online-banking).
    /// </summary>
    Service,
    /// <summary>
    /// The document provides some kind of software.
    /// </summary>
    Software,
    /// <summary>
    /// The document provides acoustic singals (e.g. a MP3
    /// file or an embedded radio stream).
    /// </summary>
    Sound,
    /// <summary>
    /// The document contains text.
    /// </summary>
    Text,
    /// <summary>
    /// Not yet specified.
    /// </summary>
    Unspecified
  }

}
