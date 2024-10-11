using System.Diagnostics.CodeAnalysis;
using GenHTTP.Api.Protocol;
using GenHTTP.Engine.Utilities;

namespace GenHTTP.Engine.Protocol;

public sealed class RequestProperties : IRequestProperties
{
    private PooledDictionary<string, object?>? _Data;

    private bool _Disposed;

    #region Get-/Setters

    public object this[string key]
    {
        get
        {
            object? result = null;

            if (Data.TryGetValue(key, out var value))
            {
                result = value;
            }

            if (result == null)
            {
                throw new KeyNotFoundException($"Key '{key}' does not exist in request properties");
            }

            return result;
        }
        set => Data[key] = value;
    }

    private PooledDictionary<string, object?> Data => _Data ??= new PooledDictionary<string, object?>();

    #endregion

    #region Functionality

    public bool TryGet<T>(string key, [MaybeNullWhen(returnValue: false)] out T entry)
    {
        if (Data.TryGetValue(key, out var value))
        {
            if (value is T result)
            {
                entry = result;
                return true;
            }
        }

        entry = default;
        return false;
    }

    public void Clear(string key)
    {
        Data[key] = null;
    }

    #endregion

    #region Disposal

    private void Dispose(bool disposing)
    {
        if (!_Disposed)
        {
            if (disposing)
            {
                _Data?.Dispose();
            }

            _Data = null;
            _Disposed = true;
        }
    }

    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    #endregion

}
