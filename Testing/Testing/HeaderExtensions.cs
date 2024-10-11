namespace GenHTTP.Testing;

public static class HeaderExtensions
{

    public static string? GetHeader(this HttpResponseMessage response, string key)
    {
        if (response.Headers.TryGetValues(key, out var values))
        {
            return values.FirstOrDefault();
        }

        return null;
    }

    public static string? GetContentHeader(this HttpResponseMessage response, string key)
    {
        if (response.Content.Headers.TryGetValues(key, out var values))
        {
            return values.FirstOrDefault();
        }

        return null;
    }
}
