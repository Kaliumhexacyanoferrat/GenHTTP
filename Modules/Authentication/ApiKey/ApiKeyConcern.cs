using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using GenHTTP.Api.Content;
using GenHTTP.Api.Content.Authentication;
using GenHTTP.Api.Protocol;

namespace GenHTTP.Modules.Authentication.ApiKey
{

    public class ApiKeyConcern : IConcern
    {

        #region Get-/Setters

        public IHandler Parent { get; }

        public IHandler Content { get; }

        private Func<IRequest, string?> KeyExtractor { get; }

        private Func<IRequest, string, ValueTask<IUser?>> Authenticator { get; }

        #endregion

        #region Initialization

        public ApiKeyConcern(IHandler parent, Func<IHandler, IHandler> contentFactory, Func<IRequest, string?> keyExtractor, Func<IRequest, string, ValueTask<IUser?>> authenticator)
        {
            Parent = parent;
            Content = contentFactory(this);

            KeyExtractor = keyExtractor;
            Authenticator = authenticator;
        }

        #endregion

        #region Functionality

        public async ValueTask<IResponse?> HandleAsync(IRequest request)
        {
            var key = KeyExtractor(request);

            if (key != null)
            {
                var user = await Authenticator(request, key).ConfigureAwait(false);

                if (user != null)
                {
                    request.SetUser(user);

                    return await Content.HandleAsync(request);
                }
                else
                {
                    return request.Respond()
                                  .Status(ResponseStatus.Forbidden)
                                  .Build();
                }
            }

            return request.Respond()
                          .Status(ResponseStatus.Unauthorized)
                          .Build();
        }

        public IEnumerable<ContentElement> GetContent(IRequest request) => Content.GetContent(request);

        public ValueTask PrepareAsync() => Content.PrepareAsync();

        #endregion

    }

}
