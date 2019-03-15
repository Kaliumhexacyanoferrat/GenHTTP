using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GenHTTP.Api.Abstraction
{

    /// <summary>
    /// Defines the available types of meta information.
    /// </summary>
    /// <remarks>
    /// The W3C standards allow the 'name' and the 
    /// 'http-equiv' attribute in a 'meta' tag at the same
    /// time. The GenHTTP object framework does not provide
    /// this feature.
    /// </remarks>
    public enum DocumentMetaInformationType
    {
        /// <summary>
        /// Meta tag with the 'name' attribute.
        /// </summary>
        Normal,
        /// <summary>
        /// Meta tag with the 'http-equiv' attribute.
        /// </summary>
        HttpEquivalent
    }

}
