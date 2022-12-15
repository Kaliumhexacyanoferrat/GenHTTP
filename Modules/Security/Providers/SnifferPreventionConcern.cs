using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using GenHTTP.Api.Content;
using GenHTTP.Api.Protocol;

namespace GenHTTP.Modules.Security.Providers
{

    public class SnifferPreventionConcern : IConcern
    {

        #region Get-/Setters

        public IHandler Parent { get; }

        public IHandler Content { get; }

        #endregion

        #region Initialization

        public SnifferPreventionConcern(IHandler parent, Func<IHandler, IHandler> contentFactory)
        {
            Parent = parent;
            Content = contentFactory(this);
        }

        #endregion

        #region Functionality

        public async ValueTask<IResponse?> HandleAsync(IRequest request)
        {
            var content = await Content.HandleAsync(request).ConfigureAwait(false);

            if (content != null)
            {
                content.Headers["X-Content-Type-Options"] = "nosniff";
            }

            return content;
        }

        public ValueTask PrepareAsync() => Content.PrepareAsync();

        public IAsyncEnumerable<ContentElement> GetContentAsync(IRequest request) => Content.GetContentAsync(request);

        #endregion

    }

}
