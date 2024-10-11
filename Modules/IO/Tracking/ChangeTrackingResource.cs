using GenHTTP.Api.Content.IO;
using GenHTTP.Api.Protocol;

namespace GenHTTP.Modules.IO.Tracking;

public sealed class ChangeTrackingResource : IResource
{
    private ulong? _LastChecksum;

    #region Initialization

    public ChangeTrackingResource(IResource source)
    {
        Source = source;
    }

    #endregion

    #region Get-/Setters

    private IResource Source { get; }

    public string? Name => Source.Name;

    public DateTime? Modified => Source.Modified;

    public FlexibleContentType? ContentType => Source.ContentType;

    public ulong? Length => Source.Length;

    #endregion

    #region Functionality

    public async ValueTask<Stream> GetContentAsync()
    {
        _LastChecksum = await CalculateChecksumAsync();

        return await Source.GetContentAsync();
    }

    public async ValueTask WriteAsync(Stream target, uint bufferSize)
    {
        _LastChecksum = await CalculateChecksumAsync();

        await Source.WriteAsync(target, bufferSize);
    }

    public ValueTask<ulong> CalculateChecksumAsync() => Source.CalculateChecksumAsync();

    /// <summary>
    /// True, if the content of the resource has changed
    /// since <see cref="GetContentAsync()" /> has been called
    /// the last time.
    /// </summary>
    public async ValueTask<bool> HasChanged() => await CalculateChecksumAsync() != _LastChecksum;

    #endregion

}
