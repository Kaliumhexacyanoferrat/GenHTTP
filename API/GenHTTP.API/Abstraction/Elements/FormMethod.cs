using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GenHTTP.Api.Abstraction.Elements
{

    /// <summary>
    /// Specifies the type of a form.
    /// </summary>
    public enum FormMethod
    {
        /// <summary>
        /// Transmit the formular data via GET.
        /// </summary>
        Get,
        /// <summary>
        /// Transmit the formular data via POST.
        /// </summary>
        Post
    }

}
