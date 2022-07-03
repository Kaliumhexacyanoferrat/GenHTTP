using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using GenHTTP.Api.Content;
using GenHTTP.Api.Infrastructure;
using GenHTTP.Api.Protocol;

using GenHTTP.Modules.Basics;

namespace GenHTTP.Modules.Security.Providers
{

    public sealed class SecurityConcern : IConcern
    {
        private const string HEADER = "X-Content-Type-Options";
        private const string VALUE_NOSNIFF = "nosniff";

        #region Get-/Setters

        public XContentTypeOptions Options { get; }

        public IHandler Parent { get; }

        public IHandler Content { get; }

        #endregion

        #region Initialization

        public SecurityConcern(IHandler parent, Func<IHandler, IHandler> contentFactory, XContentTypeOptions options)
        {
            Parent = parent;
            Content = contentFactory(this);

            Options = options;
        }

        #endregion

        #region Functionality
        
        public async ValueTask<IResponse?> HandleAsync(IRequest request)
        {
            var response = await Content.HandleAsync(request).ConfigureAwait(false);

            if (response is not null)
            {
                if (Options == XContentTypeOptions.NoSniff)
                {
                    if (!response.Headers.ContainsKey(HEADER))
                    {
                        response[HEADER] = VALUE_NOSNIFF;
                    }
                }
            }

            return response;
        }

        public ValueTask PrepareAsync() => Content.PrepareAsync();

        public IEnumerable<ContentElement> GetContent(IRequest request) => Content.GetContent(request);

        #endregion

    }

}
