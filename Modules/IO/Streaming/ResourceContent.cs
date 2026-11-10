using GenHTTP.Api.Content.IO;
using GenHTTP.Api.Protocol;

using GenHTTP.Modules.IO.Pooling;

namespace GenHTTP.Modules.IO.Streaming;

public sealed class ResourceContent : PooledContent<ResourceContent>, IResponseContent
{
    private static readonly IResource NoResource = Resource.FromString(string.Empty).Build();

    private IResource _resource = NoResource;

    #region Get-/Setters

    public ulong? Length => _resource.Length;

    public ContentType? Type { get; private set; }

    public ReadOnlyMemory<byte>? Encoding { get; private set; }

    #endregion

    #region Initialization

    public ResourceContent(IResource resource, ContentType? contentType = null, ReadOnlyMemory<byte>? encoding = null)
    {
        Apply(resource, contentType, encoding);
    }

    private ResourceContent() { }

    /// <summary>
    /// Fetches a pooled instance that serves the given resource.
    /// </summary>
    /// <remarks>
    /// The instance is returned to the pool when the server disposes it after
    /// writing the response, so it must be passed to exactly one response.
    /// </remarks>
    /// <param name="resource">The resource to be served</param>
    /// <param name="contentType">The content type of the resource (guessed from the resource, if not given)</param>
    /// <param name="encoding">The content encoding of the resource, if it is already compressed</param>
    /// <returns>The pooled content serving the given resource</returns>
    /// <example>
    /// <code>
    /// request.Respond().Content(ResourceContent.Rent(Resource.FromFile("index.html").Build())).Build();
    /// </code>
    /// </example>
    public static ResourceContent Rent(IResource resource, ContentType? contentType = null, ReadOnlyMemory<byte>? encoding = null)
    {
        var content = FromPool(static () => new ResourceContent());

        content.Apply(resource, contentType, encoding);

        return content;
    }

    private void Apply(IResource resource, ContentType? contentType, ReadOnlyMemory<byte>? encoding)
    {
        _resource = resource;
        Type = contentType ?? resource.GuessContentType();
        Encoding = encoding;
    }

    #endregion

    #region Functionality

    public async ValueTask<ulong?> CalculateChecksumAsync() => await _resource.CalculateChecksumAsync();

    public ValueTask WriteAsync(IResponseSink sink) => _resource.WriteAsync(sink);

    protected override void Reset()
    {
        _resource = NoResource;
        Type = null;
        Encoding = null;
    }

    #endregion

}
