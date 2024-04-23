using System.Collections.Generic;
using System.Threading.Tasks;

using GenHTTP.Api.Content;
using GenHTTP.Api.Protocol;

using GenHTTP.Modules.Basics;

namespace GenHTTP.Modules.Pages.Combined
{

    public class CombinedPageProvider : IHandler
    {

        #region Get-/Setters

        public IHandler Parent { get; }

        private ContentInfo ContentInfo { get; }

        private PageAdditions? Additions { get; }

        private ResponseModifications? Modifications { get; }

        private List<PageFragment> Fragments { get; }

        #endregion

        #region Initialization

        public CombinedPageProvider(IHandler parent, ContentInfo contentInfo, PageAdditions? additions, ResponseModifications? modifications, List<PageFragment> fragments)
        {
            Parent = parent;

            ContentInfo = contentInfo;
            Additions = additions;
            Modifications = modifications;

            Fragments = fragments;
        }

        #endregion

        #region Functionality

        public async ValueTask PrepareAsync()
        {
            foreach (var fragment in Fragments)
            {
                await fragment.Renderer.PrepareAsync();
            }
        }

        public IAsyncEnumerable<ContentElement> GetContentAsync(IRequest request) => this.GetContent(request, ContentInfo, ContentType.TextHtml);

        public async ValueTask<IResponse?> HandleAsync(IRequest request)
        {
            var contentFragments = new List<ContentFragment>(Fragments.Count);

            foreach (var fragment in Fragments)
            {
                contentFragments.Add(new ContentFragment(fragment.Renderer, await fragment.Model(request, this)));
            }

            var content = new CombinedPageContent(contentFragments, ContentInfo, Additions, this, request);

            var response = request.Respond()
                                  .Content(content)
                                  .Type(ContentType.TextHtml);

            if (Modifications != null)
            {
                Modifications.Apply(response);
            }

            return response.Build();
        }

        #endregion

    }

}
