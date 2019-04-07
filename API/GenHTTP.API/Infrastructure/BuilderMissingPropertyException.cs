using System;
using System.Collections.Generic;
using System.Text;

namespace GenHTTP.Api.Infrastructure
{

    /// <summary>
    /// Will be thrown, if a builder is missing a required property
    /// that is needed to create the target instance.
    /// </summary>
    public class BuilderMissingPropertyException : Exception
    {

        #region Get-/Setters

        /// <summary>
        /// The name of the property which has not been set.
        /// </summary>
        public string Property { get; }

        #endregion

        #region Initialization

        public BuilderMissingPropertyException(string property) : base($"Missing required property '{property}'")
        {
            Property = property;
        }

        #endregion

    }

}
