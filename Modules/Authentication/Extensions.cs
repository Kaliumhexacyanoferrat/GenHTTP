using GenHTTP.Api.Content.Authentication;
using GenHTTP.Api.Protocol;

namespace GenHTTP.Modules.Authentication
{

    public static class Extensions
    {
        private const string USER_PROPERTY = "__AUTH_USER";

        public static void SetUser(this IRequest request, IUser user)
        {
            request.Properties[USER_PROPERTY] = user;
        }

        public static T? GetUser<T>(this IRequest request) where T : class, IUser
        {
            if (request.Properties.TryGet<T>(USER_PROPERTY, out var user))
            {
                return user;
            }

            return null;
        }

    }

}
