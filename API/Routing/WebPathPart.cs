using System;

namespace GenHTTP.Api.Routing
{

    /// <summary>
    /// Represents a part of an URL (between two slashes).
    /// </summary>
    public class WebPathPart
    {

        #region Get-/Setters

        /// <summary>
        /// The string as received by the server (e.g. "some%20path").
        /// </summary>
        public string Original { get; }

        /// <summary>
        /// The decoded representation of the path (e.g. "some path").
        /// </summary>
        public string Value { get; }

        #endregion

        #region Initialization

        /// <summary>
        /// Creates a new part from the original string.
        /// </summary>
        /// <param name="original">The original string</param>
        public WebPathPart(string original)
        {
            Original = original;
            Value = (original.Contains('%')) ? Uri.UnescapeDataString(original) : original;
        }

        #endregion

        #region Functionality

        public override string ToString() => Value;

        public override int GetHashCode()
        {
            unchecked
            {
                var hash = 17;

                hash = hash * 23 + Original.GetHashCode();
                hash = hash * 23 + Value.GetHashCode();

                return hash;
            }
        }

        public static bool operator ==(WebPathPart part, string value) => part.Original == value || part.Value == value;

        public static bool operator !=(WebPathPart part, string value) => part.Original != value && part.Value != value;

        public override bool Equals(object? obj)
        {
            if (ReferenceEquals(this, obj))
            {
                return true;
            }

            if (obj is null)
            {
                return false;
            }

            return (obj as WebPathPart)?.GetHashCode() == GetHashCode();
        }

        #endregion

    }

}
