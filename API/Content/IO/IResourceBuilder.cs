using GenHTTP.Api.Infrastructure;
using GenHTTP.Api.Protocol;

namespace GenHTTP.Api.Content.IO;

/// <summary>
/// When implemented by builders providing resource instances,
/// this interface allows to configure common properties of
/// resources in a unified way.
/// </summary>
public interface IResourceBuilder<out T> : IBuilder<IResource> where T : IResourceBuilder<T>
{

    /// <summary>
    /// Sets the name of the resource.
    /// </summary>
    /// <param name="name">The name of the resource</param>
    T Name(string name);

    /// <summary>
    /// Sets the content type of the resource.
    /// </summary>
    /// <param name="contentType">The content type of the resource</param>
    T Type(ContentType contentType);

    /// <summary>
    /// Sets the modification date and time of the resource.
    /// </summary>
    /// <param name="modified">The modification date and time of the resource</param>
    T Modified(DateTime modified);
    
}
