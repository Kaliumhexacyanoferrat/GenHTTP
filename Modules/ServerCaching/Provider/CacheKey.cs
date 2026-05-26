using GenHTTP.Api.Protocol;

namespace GenHTTP.Modules.ServerCaching.Provider;

public static class CacheKey
{
    
    public static string GetKey(this IRequest request)
    {
        unchecked
        {
            var hash = new HashCode();
            
            var header = request.Header;

            hash.Add(header.Target.AsString(false));

            for (var i = 0; i < header.Query.Count; i++)
            {
                var arg = header.Query[i];

                hash.AddBytes(arg.Key.Span);
                hash.AddBytes(arg.Value.Span);
            }

            var host = request.Header.Headers.GetEntry(KnownHeaders.Host);

            if (host != null)
            {
                hash.AddBytes(host.Value.Span);
            }

            return hash.ToHashCode().ToString();
        }
    }

    public static string GetVariationKey(this Dictionary<string, string>? variations)
    {
        unchecked
        {
            ulong hash = 17;

            if (variations != null)
            {
                foreach (var arg in variations)
                {
                    hash = hash * 23 + (ulong)arg.Key.GetHashCode();
                    hash = hash * 23 + (ulong)arg.Value.GetHashCode();
                }
            }

            return hash.ToString();
        }
    }

}
