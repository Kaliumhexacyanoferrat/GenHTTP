using System;
using System.Runtime.Serialization;

namespace GenHTTP.Api.Infrastructure
{

    /// <summary>
    /// Will be thrown, if a builder is missing a required property
    /// that is needed to create the target instance.
    /// </summary>
    [Serializable]
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

        protected BuilderMissingPropertyException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
            Property = info.GetString("Property") ?? string.Empty;
        }

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            info.AddValue("Property", Property);
        }

        #endregion

    }

}
