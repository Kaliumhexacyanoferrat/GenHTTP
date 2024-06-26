﻿using GenHTTP.Api.Protocol;

namespace GenHTTP.Modules.Authentication.Web.Integration
{
    
    /// <summary>
    /// A session handling implementation that will use cookies to 
    /// store and fetch the session tokens.
    /// </summary>
    public class CookieSessionHandling : ISessionHandling
    {
        private const string COOKIE_NAME = "wa_session";

        private const ulong COOKIE_TIMEOUT = 2592000; // 30d

        public string? ReadToken(IRequest request)
        {
            if (request.Cookies.TryGetValue(COOKIE_NAME, out var token))
            {
                return new(token.Value);
            }

            return null;
        }

        public void WriteToken(IResponse response, string sessionToken)
        {
            response.SetCookie(new(COOKIE_NAME, sessionToken, COOKIE_TIMEOUT));
        }

        public void ClearToken(IResponse response)
        {
            response.SetCookie(new(COOKIE_NAME, "", 0));
        }

    }

}
