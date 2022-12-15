using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using GenHTTP.Api.Content;
using GenHTTP.Api.Content.Websites;
using GenHTTP.Api.Protocol;

namespace GenHTTP.Modules.Websites.Menues
{

    public sealed class GeneratedMenuProvider : IMenuProvider
    {

        #region Get-/Setters

        private Func<IRequest, IHandler, IAsyncEnumerable<ContentElement>> Provider { get; }

        #endregion

        #region Initialization 

        public GeneratedMenuProvider(Func<IRequest, IHandler, IAsyncEnumerable<ContentElement>> provider)
        {
            Provider = provider;
        }

        #endregion

        #region Functionality

        public async ValueTask<List<ContentElement>> GetMenuAsync(IRequest request, IHandler handler)
        {
            var result = new List<ContentElement>();

            await foreach (var content in Provider(request, handler))
            {
                if (content.ContentType == ContentType.TextHtml)
                {
                    result.Add(content);
                }
            }

            return result;
        }

        #endregion

    }

}
