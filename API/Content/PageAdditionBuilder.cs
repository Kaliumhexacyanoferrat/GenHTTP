using GenHTTP.Api.Infrastructure;

namespace GenHTTP.Api.Content
{

    public class PageAdditionBuilder : IBuilder<PageAdditions?>, IPageAdditionBuilder<PageAdditionBuilder>
    {
        private PageAdditions? _Additions;

        public PageAdditionBuilder AddScript(string path, bool asynchronous = false)
        {
            EnsureAdditions().Scripts.Add(new(path, asynchronous));
            return this;
        }

        public PageAdditionBuilder AddStyle(string path)
        {
            EnsureAdditions().Styles.Add(new(path));
            return this;
        }

        private PageAdditions EnsureAdditions() => _Additions ?? (_Additions = PageAdditions.Create());

        public PageAdditions? Build() => _Additions;

    }

}
