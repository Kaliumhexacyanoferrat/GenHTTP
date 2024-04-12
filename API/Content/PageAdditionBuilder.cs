using GenHTTP.Api.Infrastructure;

namespace GenHTTP.Api.Content
{

    /// <summary>
    /// Can be used by handler builders to create a page addition object
    /// so that they can easily offer the page addition feature
    /// without the need of implementing everything on their own.
    /// </summary>
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

        private PageAdditions EnsureAdditions() => _Additions ??= PageAdditions.Create();

        public PageAdditions? Build() => _Additions;

    }

}
