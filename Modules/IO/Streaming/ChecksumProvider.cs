namespace GenHTTP.Modules.IO.Streaming;

/// <summary>
/// Centralizes checksum handling of binary data by providing a cache.
/// </summary>
public class ChecksumProvider
{
    private readonly Func<ValueTask<ulong?>> _provider;

    #region Get-/Setters

    private bool ChecksumComputed { get; set; }

    private ulong? CachedChecksum { get; set; }

    #endregion

    #region Initialization

    /// <summary>
    /// Creates a new checksum provider that will back the given computation logic.
    /// </summary>
    /// <param name="provider">The checksum provider to invoke</param>
    public ChecksumProvider(Func<ValueTask<ulong?>> provider)
    {
        _provider = provider;
    }

    #endregion

    #region Functionality

    public ValueTask<ulong?> Compute()
    {
        if (ChecksumComputed)
        {
            return new(CachedChecksum);
        }

        return ComputeAndCache();
    }

    private async ValueTask<ulong?> ComputeAndCache()
    {
        var checksum = await _provider();

        CachedChecksum = checksum;
        ChecksumComputed = true;

        return checksum;
    }

    #endregion

}
