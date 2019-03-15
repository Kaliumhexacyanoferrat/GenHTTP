using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GenHTTP.Abstraction.Compiling
{

    /// <summary>
    /// A template, used to seperate static from dynamic content.
    /// </summary>
    public interface ITemplate
    {

        /// <summary>
        /// Retrieve the whole content of the template.
        /// </summary>
        /// <returns></returns>
        byte[] ToByteArray();

        /// <summary>
        /// Retrieve the template base of this template.
        /// </summary>
        ITemplateBase Base { get; }

    }

}
