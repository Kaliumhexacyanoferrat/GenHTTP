using System.Collections.Generic;
using System.Threading.Tasks;

using GenHTTP.Api.Content;
using GenHTTP.Api.Content.IO;
using GenHTTP.Api.Content.Templating;

using GenHTTP.Api.Protocol;

using GenHTTP.Modules.Basics;
using GenHTTP.Modules.IO;

namespace GenHTTP.Modules.Placeholders.Providers
{

    public sealed class PageProvider : IHandler
    {

        #region Get-/Setters

        public IHandler Parent { get; }

        public ContentInfo PageInfo { get; }

        public IResource Content { get; }

        #endregion

        #region Initialization

        public PageProvider(IHandler parent, ContentInfo pageInfo, IResource content)
        {
            Parent = parent;

            PageInfo = pageInfo;
            Content = content;
        }

        #endregion

        #region Functionality

        public async ValueTask<IResponse?> HandleAsync(IRequest request)
        {
            var templateModel = new TemplateModel(request, this, PageInfo, await Content.GetResourceAsStringAsync().ConfigureAwait(false));

            var page = await this.GetPageAsync(request, templateModel).ConfigureAwait(false);
             
            return page.Build();
        }

        public ValueTask PrepareAsync() => ValueTask.CompletedTask;

        public IAsyncEnumerable<ContentElement> GetContentAsync(IRequest request) => this.GetContent(request, PageInfo, ContentType.TextHtml);

        #endregion

    }

}
