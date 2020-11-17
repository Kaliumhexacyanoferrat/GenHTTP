using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

using GenHTTP.Api.Content;
using GenHTTP.Api.Content.Authentication;
using GenHTTP.Api.Protocol;

namespace GenHTTP.Modules.Authentication.Basic
{

    public class BasicAuthenticationConcern : IConcern
    {

        #region Get-/Setters

        public IHandler Content { get; }

        public IHandler Parent { get; }

        private string Realm { get; }

        private Func<string, string, ValueTask<IUser?>> Authenticator { get; }

        #endregion

        #region Initialization

        public BasicAuthenticationConcern(IHandler parent, Func<IHandler, IHandler> contentFactory, string realm, Func<string, string, ValueTask<IUser?>> authenticator)
        {
            Parent = parent;
            Content = contentFactory(this);

            Realm = realm;
            Authenticator = authenticator;
        }

        #endregion

        #region Functionality

        public IEnumerable<ContentElement> GetContent(IRequest request) => Content.GetContent(request);

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

            if (!TryDecode(authHeader.Substring(6), out var credentials))
            {
                return GetChallenge(request);
            }

            var user = await Authenticator(credentials.username, credentials.password).ConfigureAwait(false);

            if (user is null)
            {
                return GetChallenge(request);
            }

            request.SetUser(user);

            return await Content.HandleAsync(request).ConfigureAwait(false);
        }

        private IResponse GetChallenge(IRequest request)
        {
            return request.Respond()
                          .Status(ResponseStatus.Unauthorized)
                          .Header("WWW-Authenticate", $"Basic realm=\"{Realm}\", charset=\"UTF-8\"")
                          .Build();
        }

        private bool TryDecode(string header, out (string username, string password) credentials)
        {
            try
            {
                var bytes = Convert.FromBase64String(header);
                var str = Encoding.UTF8.GetString(bytes);

                var colon = str.IndexOf(':');

                if ((colon > -1) && (str.Length > colon))
                {
                    credentials = (str.Substring(0, colon), str.Substring(colon + 1));
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

}
