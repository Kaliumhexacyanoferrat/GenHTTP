using System.Diagnostics.CodeAnalysis;
using GenHTTP.Api.Protocol;

namespace GenHTTP.Benchmarks.Infrastructure.Context;

public class Properties : Dictionary<string, object>, IRequestProperties
{

    public bool TryGet<T>(string key, [MaybeNullWhen(false)] out T entry)
    {
        if (TryGetValue(key, out var value))
        {
            if (value is T typed)
            {
                entry = typed;
                return true;
            }
        }

        entry = default;
        return false;
    }

    public void Clear(string key)
    {
        Remove(key);
    }

}
