using System.Diagnostics.CodeAnalysis;

namespace GenHTTP.Api.Protocol
{

    /// <summary>
    /// Property bag to store additional data within the
    /// currently running request context.
    /// </summary>
    public interface IRequestProperties
    {

        /// <summary>
        /// Accesses a value that is stored within
        /// the property bag.
        /// </summary>
        /// <param name="key">The key of the item to be fetched</param>
        /// <exception cref="System.Collections.Generic.KeyNotFoundException">Thrown if the given key is not present</exception>
        object this[string key] { get; set; }

        /// <summary>
        /// Attempts to fetch the typed value for the given key from the property bag.
        /// </summary>
        /// <param name="key">The key of the value to be fetched</param>
        /// <param name="entry">The entry read from the property bag, if any</param>
        /// <typeparam name="T">The expected type of value to be returned</typeparam>
        /// <returns>True if the value could be read, false otherwise</returns>
        bool TryGet<T>(string key, [MaybeNullWhen(returnValue: false)] out T entry);

    }

}
