using System;
using System.IO;
using System.Threading.Tasks;
using GenHTTP.Api.Protocol;

namespace GenHTTP.Api.Content.IO
{

    /// <summary>
    /// Allows content providers to access a resource without the need
    /// to know the actual way of access (such as database, web, or
    /// file system).
    /// </summary>
    /// <remarks>
    /// As resources may change (e.g. if an user changes the file that
    /// is provided as a resource), content providers must not
    /// cache the results of a method call.
    /// </remarks>
    public interface IResource
    {

        /// <summary>
        /// The name of this resource, if known. 
        /// </summary>
        string? Name { get; }

        /// <summary>
        /// The point in time, when the resource was last modified, if known.
        /// </summary>
        DateTime? Modified { get; }

        /// <summary>
        /// The content type of this resource, if known.
        /// </summary>
        FlexibleContentType? ContentType { get; }

        /// <summary>
        /// The number of bytes provided by this resource.
        /// </summary>
        /// <remarks>
        /// This field will not be used to control the HTTP flow, but
        /// just as meta information (e.g. to be rendered by the
        /// directory listing handler). For optimzed data transfer,
        /// the stream provided by this resource should be seekable
        /// and return a sane length.
        /// </remarks>
        ulong? Length { get; }

        /// <summary>
        /// Calculates the checksum of the resource.
        /// </summary>
        /// <returns>The checksum of the resource</returns>
        ValueTask<ulong> CalculateChecksumAsync();

        /// <summary>
        /// Returns the read-only stream of the resource to be accessed.
        /// </summary>
        /// <returns>The resource to be accessed</returns>
        ValueTask<Stream> GetContentAsync();

    }

}
