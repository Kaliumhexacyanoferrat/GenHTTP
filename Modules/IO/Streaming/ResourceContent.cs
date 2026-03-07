using GenHTTP.Api.Content.IO;
using GenHTTP.Api.Protocol;

namespace GenHTTP.Modules.IO.Streaming;

public sealed class ResourceContent : IResponseContent
{

    #region Initialization

    public ResourceContent(IResource resource)
    {
        Resource = resource;
    }

    #endregion

    #region Get-/Setters

    public ulong? Length => Resource.Length;

    private IResource Resource { get; }

    #endregion

    #region Functionality

    public async ValueTask<ulong?> CalculateChecksumAsync() => await Resource.CalculateChecksumAsync();

    public ValueTask WriteAsync(Stream target, uint bufferSize) => Resource.WriteAsync(target, bufferSize);

    public ValueTask WriteAsync(IResponseSink sink)
    {
        Resource.Write(sink.Writer);
        return ValueTask.CompletedTask;
    }

    #endregion

}
