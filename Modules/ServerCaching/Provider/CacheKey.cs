using GenHTTP.Api.Protocol;

namespace GenHTTP.Modules.ServerCaching.Provider;

public static class CacheKey
{

    public static string GetKey(this IRequest request)
    {
        unchecked
        {
            var header = request.Raw.Header;

            ulong hash = 17;

            hash = hash * 23 + (ulong)header.Target.AsString(false).GetHashCode();

            for (var i = 0; i < header.Query.Count; i++)
            {
                var arg = header.Query[i];

                hash = hash * 23 + (ulong)arg.Key.GetHashCode(); // todo: this is unstable accross restarts
                hash = hash * 23 + (ulong)arg.Value.GetHashCode();
            }

            hash = hash * 23 + (ulong)request.Host.GetHashCode();

            return hash.ToString();
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
