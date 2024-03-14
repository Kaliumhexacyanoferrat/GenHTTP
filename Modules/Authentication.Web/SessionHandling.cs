using System;
using System.Threading.Tasks;

using GenHTTP.Api.Content.Authentication;
using GenHTTP.Api.Protocol;

namespace GenHTTP.Modules.Authentication.Web
{

    public record class SessionConfig
    (

        Func<IRequest, ValueTask<string?>> ReadToken,

        Action<IResponseBuilder, string> WriteToken,

        Action<IResponse> ClearToken,

        Func<string, ValueTask<IUser?>> VerifyToken

    );

    public static class SessionHandling
    {
        private const string COOKIE_NAME = "wa_session";

        private const ulong COOKIE_TIMEOUT = 2592000; // 30d

        public static SessionConfig BuiltIn(Func<string, ValueTask<IUser?>> verifyToken)
        {
            return new(ReadToken, WriteToken, ClearToken, verifyToken);
        }

        public static SessionConfig Custom(Func<IRequest, ValueTask<string?>> readToken, Action<IResponseBuilder, string> writeToken, Action<IResponse> clearToken, Func<string, ValueTask<IUser?>> verifyToken)
        {
            return new(readToken, writeToken, clearToken, verifyToken);
        }

        private static ValueTask<string?> ReadToken(IRequest request)
        {
            if (request.Cookies.TryGetValue(COOKIE_NAME, out var token))
            {
                return new(token.Value);
            }

            return new();
        }

        private static void WriteToken(IResponseBuilder response, string value)
        {
            response.Cookie(new(COOKIE_NAME, value, COOKIE_TIMEOUT));
        }

        private static void ClearToken(IResponse response)
        {
            response.SetCookie(new(COOKIE_NAME, string.Empty, 0));
        }

    }

}
