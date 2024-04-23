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

        public PageAdditions? Additions { get; }

        public ResponseModifications? Modifications { get; }

        public IResource Content { get; }

        #endregion

        #region Initialization

        public PageProvider(IHandler parent, ContentInfo pageInfo, PageAdditions? additions, ResponseModifications? modifications, IResource content)
        {
            Parent = parent;

            PageInfo = pageInfo;
            Additions = additions;
            Modifications = modifications;

            Content = content;
        }

        #endregion

        #region Functionality

        public async ValueTask<IResponse?> HandleAsync(IRequest request)
        {
            var templateModel = new TemplateModel(request, this, PageInfo, Additions, await Content.GetResourceAsStringAsync());

            var page = await this.GetPageAsync(request, templateModel);

            Modifications?.Apply(page);
             
            return page.Build();
        }

        public ValueTask PrepareAsync() => ValueTask.CompletedTask;

        public IAsyncEnumerable<ContentElement> GetContentAsync(IRequest request) => this.GetContent(request, PageInfo, ContentType.TextHtml);

        #endregion

    }

}
