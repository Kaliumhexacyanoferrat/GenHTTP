using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GenHTTP.Api.Abstraction.Elements
{

    /// <summary>
    /// Specifies the type of an form element.
    /// </summary>
    public enum InputType
    {
        /// <summary>
        /// A text field.
        /// </summary>
        Text,
        /// <summary>
        /// A password field.
        /// </summary>
        Password,
        /// <summary>
        /// A checkbox.
        /// </summary>
        Checkbox,
        /// <summary>
        /// A radio button.
        /// </summary>
        Radio,
        /// <summary>
        /// A submit button.
        /// </summary>
        Submit,
        /// <summary>
        /// A reset button.
        /// </summary>
        Reset,
        /// <summary>
        /// A file upload dialog.
        /// </summary>
        File,
        /// <summary>
        /// A hidden field.
        /// </summary>
        Hidden,
        /// <summary>
        /// An image.
        /// </summary>
        Image,
        /// <summary>
        /// A button.
        /// </summary>
        Button
    }

}
