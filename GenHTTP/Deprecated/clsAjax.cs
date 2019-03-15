using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GenHTTP {

  /// <summary>
  /// Represents an AJAX response page.
  /// </summary>
  public interface IAjax {

    /// <summary>
    /// Retrieve the content of the AJAX page.
    /// </summary>
    byte[] SerializedContent { get; }

  }

}
