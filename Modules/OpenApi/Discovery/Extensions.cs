namespace GenHTTP.Modules.OpenApi.Discovery;

internal static class Extensions
{

    internal static bool MightBeNull(this Type type)
    {
        if (type.IsClass)
        {
            return true;
        }

        return Nullable.GetUnderlyingType(type) != null;
    }

}
