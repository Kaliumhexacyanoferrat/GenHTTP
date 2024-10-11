using GenHTTP.Api.Content.Authentication;
using GenHTTP.Api.Protocol;

namespace GenHTTP.Modules.Authentication;

public static class Extensions
{
    private const string UserProperty = "__AUTH_USER";

    public static void SetUser(this IRequest request, IUser user)
    {
        request.Properties[UserProperty] = user;
    }

    public static T? GetUser<T>(this IRequest request) where T : class, IUser
    {
        return request.Properties.TryGet<T>(UserProperty, out var user) ? user : null;
    }

    public static void ClearUser(this IRequest request)
    {
        request.Properties.Clear(UserProperty);
    }

}
