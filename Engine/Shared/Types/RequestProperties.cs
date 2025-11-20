using System.Diagnostics.CodeAnalysis;
using GenHTTP.Api.Protocol;

namespace GenHTTP.Engine.Shared.Types;

public sealed class RequestProperties : IRequestProperties
{
    private readonly Dictionary<string, object?> _data = new();

    #region Get-/Setters

    public object this[string key]
    {
        get
        {
            object? result = null;

            if (_data.TryGetValue(key, out var value))
            {
                result = value;
            }

            if (result == null)
            {
                throw new KeyNotFoundException($"Key '{key}' does not exist in request properties");
            }

            return result;
        }
        set => _data[key] = value;
    }

    #endregion

    #region Functionality

    public bool TryGet<T>(string key, [MaybeNullWhen(returnValue: false)] out T entry)
    {
        if (_data.TryGetValue(key, out var value))
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
        _data[key] = null;
    }

    public void Clear()
    {
        _data.Clear();
    }

    #endregion

}
