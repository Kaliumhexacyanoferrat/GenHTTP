using GenHTTP.Api.Content.IO;
using GenHTTP.Api.Protocol;

namespace GenHTTP.Modules.IO.Tracking;

public sealed class ChangeTrackingResource : IResource
{
    private ulong? _lastChecksum;

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
        _lastChecksum = await CalculateChecksumAsync();

        return await Source.GetContentAsync();
    }

    public async ValueTask WriteAsync(Stream target, uint bufferSize)
    {
        _lastChecksum = await CalculateChecksumAsync();

        await Source.WriteAsync(target, bufferSize);
    }

    public ValueTask<ulong> CalculateChecksumAsync() => Source.CalculateChecksumAsync();

    /// <summary>
    /// Checks whether the content of the resource has changed
    /// since <see cref="GetContentAsync()" /> or <see cref="WriteAsync(Stream, uint)"/>
    /// has been called the last time.
    /// </summary>
    /// <returns>True if the content has changed, false otherwise</returns>
    public async ValueTask<bool> CheckChangedAsync() => await CalculateChecksumAsync() != _lastChecksum;

    /// <summary>
    /// True, if the content of the resource has changed
    /// since <see cref="GetContentAsync()" /> has been called
    /// the last time.
    /// </summary>
    [Obsolete("Use CheckChangedAsync() instead. This method will be removed in GenHTTP 11.")]
    public ValueTask<bool> HasChanged() => CheckChangedAsync();

    #endregion

}
