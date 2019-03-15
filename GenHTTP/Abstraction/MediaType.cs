/*

Updated: 2009/10/16

2009/10/16  Andreas Nägeli        Initial version of this file.


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

namespace GenHTTP.Abstraction {
  
  /// <summary>
  /// Defines available output devices.
  /// </summary>
  /// <remarks>
  /// This enumeration provides the <see href="http://www.w3.org/TR/html4/types.html#h-6.13">media descriptors
  /// of the DTD</see>.
  /// </remarks>
  public enum MediaType {
    /// <summary>
    /// Suitable for all devices.
    /// </summary>
    All,
    /// <summary>
    /// Intended for handheld devices (small screen, monochrome,
    /// bitmapped graphics, limited bandwidth).
    /// </summary>
    Handheld,
    /// <summary>
    /// Intended for paged, opaque material and for documents
    /// viewed on screen in print preview mode.
    /// </summary>
    Print,
    /// <summary>
    /// Intended for non-paged computer screens.
    /// </summary>
    Screen,
    /// <summary>
    /// Intended for media using a fixed-pitch character grid,
    /// such as teletypes, terminals, or portable devices with
    /// limited display capabilities.
    /// </summary>
    TTY,
    /// <summary>
    /// Intended for television-type devices (low resolution,
    /// color, limited scrollability).
    /// </summary>
    TV,
    /// <summary>
    /// Intended for projectors.
    /// </summary>
    Projection,
    /// <summary>
    /// Intended for braille tactile feedback devices.
    /// </summary>
    Braille,
    /// <summary>
    /// Intended for speech synthesizers.
    /// </summary>
    Aural,
    /// <summary>
    /// Unspecified device.
    /// </summary>
    Unspecified
  }

}
