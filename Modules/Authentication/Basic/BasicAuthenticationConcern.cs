﻿using System.Text;

using GenHTTP.Api.Content;
using GenHTTP.Api.Content.Authentication;
using GenHTTP.Api.Protocol;

namespace GenHTTP.Modules.Authentication.Basic;

public sealed class BasicAuthenticationConcern : IConcern
{

    #region Get-/Setters

    public IHandler Content { get; }

    private string Realm { get; }

    private Func<string, string, ValueTask<IUser?>> Authenticator { get; }

    #endregion

    #region Initialization

    public BasicAuthenticationConcern(IHandler content, string realm, Func<string, string, ValueTask<IUser?>> authenticator)
    {
        Content = content;

        Realm = realm;
        Authenticator = authenticator;
    }

    #endregion

    #region Functionality

    public ValueTask PrepareAsync() => Content.PrepareAsync();

    public async ValueTask<IResponse?> HandleAsync(IRequest request)
    {
        if (!request.Headers.TryGetValue("Authorization", out var authHeader))
        {
            return GetChallenge(request);
        }

        if (!authHeader.StartsWith("Basic "))
        {
            return GetChallenge(request);
        }

        if (!TryDecode(authHeader[6..], out var credentials))
        {
            return GetChallenge(request);
        }

        var user = await Authenticator(credentials.username, credentials.password);

        if (user is null)
        {
            return GetChallenge(request);
        }

        request.SetUser(user);

        return await Content.HandleAsync(request);
    }

    private IResponse GetChallenge(IRequest request) => request.Respond()
                                                               .Status(ResponseStatus.Unauthorized)
                                                               .Header("WWW-Authenticate", $"Basic realm=\"{Realm}\", charset=\"UTF-8\"")
                                                               .Build();

    private static bool TryDecode(string header, out (string username, string password) credentials)
    {
        try
        {
            var bytes = Convert.FromBase64String(header);
            var str = Encoding.UTF8.GetString(bytes);

            var colon = str.IndexOf(':');

            if (colon > -1 && str.Length > colon)
            {
                credentials = (str[..colon], str[(colon + 1)..]);
                return true;
            }

        }
        catch (FormatException)
        {
            // invalid base 64 encoded string
        }

        credentials = (string.Empty, string.Empty);
        return false;
    }

    #endregion

}
