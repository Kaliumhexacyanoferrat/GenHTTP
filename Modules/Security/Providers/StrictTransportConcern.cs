using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using GenHTTP.Api.Content;
using GenHTTP.Api.Protocol;

namespace GenHTTP.Modules.Security.Providers
{

    public sealed class StrictTransportConcern : IConcern
    {
        private const string HEADER = "Strict-Transport-Security";

        #region Get-/Setters

        public IHandler Parent { get; }

        public IHandler Content { get; }

        public StrictTransportPolicy Policy { get; }

        private string HeaderValue { get; }

        #endregion

        #region Initialization

        public StrictTransportConcern(IHandler parent, Func<IHandler, IHandler> contentFactory, StrictTransportPolicy policy)
        {
            Parent = parent;
            Content = contentFactory(this);

            Policy = policy;
            HeaderValue = GetPolicyHeader();
        }

        #endregion

        #region Functionality

        public async ValueTask<IResponse?> HandleAsync(IRequest request)
        {
            var response = await Content.HandleAsync(request).ConfigureAwait(false);

            if (response is not null)
            {
                if (request.EndPoint.Secure)
                {
                    if (!response.Headers.ContainsKey(HEADER))
                    {
                        response[HEADER] = HeaderValue;
                    }
                }
            }

            return response;
        }

        private string GetPolicyHeader()
        {
            var seconds = (int)Policy.MaximumAge.TotalSeconds;

            var result = $"max-age={seconds}";

            if (Policy.IncludeSubdomains)
            {
                result += "; includeSubDomains";
            }

            if (Policy.Preload)
            {
                result += "; preload";
            }

            return result;
        }

        public ValueTask PrepareAsync() => Content.PrepareAsync();

        public IAsyncEnumerable<ContentElement> GetContentAsync(IRequest request) => Content.GetContentAsync(request);

        #endregion

    }

}
