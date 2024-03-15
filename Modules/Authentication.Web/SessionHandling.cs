using System;
using System.Threading.Tasks;

using GenHTTP.Api.Content.Authentication;
using GenHTTP.Api.Protocol;

namespace GenHTTP.Modules.Authentication.Web
{

    public record class SessionConfig
    (

        Func<IRequest, ValueTask<string?>> ReadToken,

        Action<IResponse, string> WriteToken,

        Action<IResponse> ClearToken,

        Func<string, ValueTask<IUser?>> VerifyToken,

        Func<IRequest, IUser, ValueTask<string>> StartSession

    );

    public static class SessionHandling
    {
        private const string CONFIG_KEY = "__AUTH_WEB_SESSIONCONFIG";

        private const string COOKIE_NAME = "wa_session";

        private const ulong COOKIE_TIMEOUT = 2592000; // 30d

        public static SessionConfig BuiltIn(Func<string, ValueTask<IUser?>> verifyToken, Func<IRequest, IUser, ValueTask<string>> startSession)
        {
            return new(ReadToken, WriteToken, ClearToken, verifyToken, startSession);
        }

        public static SessionConfig Custom(Func<IRequest, ValueTask<string?>> readToken, Action<IResponse, string> writeToken, Action<IResponse> clearToken, Func<string, ValueTask<IUser?>> verifyToken, Func<IRequest, IUser, ValueTask<string>> startSession)
        {
            return new(readToken, writeToken, clearToken, verifyToken, startSession);
        }

        private static ValueTask<string?> ReadToken(IRequest request)
        {
            if (request.Cookies.TryGetValue(COOKIE_NAME, out var token))
            {
                return new(token.Value);
            }

            return new();
        }

        private static void WriteToken(IResponse response, string value)
        {
            response.SetCookie(new(COOKIE_NAME, value, COOKIE_TIMEOUT));
        }

        private static void ClearToken(IResponse response)
        {
            response.SetCookie(new(COOKIE_NAME, string.Empty, 0));
        }

        public static SessionConfig GetConfig(IRequest request) => (SessionConfig)request.Properties[CONFIG_KEY];

        public static void SetConfig(IRequest request, SessionConfig config) => request.Properties[CONFIG_KEY] = config;

    }

}
