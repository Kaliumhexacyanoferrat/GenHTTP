using System.Collections.Generic;

using GenHTTP.Api.Infrastructure;
using GenHTTP.Api.Content;
using GenHTTP.Api.Content.IO;

namespace GenHTTP.Modules.Placeholders.Providers
{

    public class PageProviderBuilder : IHandlerBuilder<PageProviderBuilder>, IContentInfoBuilder<PageProviderBuilder>
    {
        private IResource? _Content;

        private readonly ContentInfoBuilder _Info = new();

        private readonly List<IConcernBuilder> _Concerns = new();

        #region Functionality

        public PageProviderBuilder Title(string title)
        {
            _Info.Title(title);
            return this;
        }

        public PageProviderBuilder Description(string description)
        {
            _Info.Description(description);
            return this;
        }

        public PageProviderBuilder Content(IResource templateProvider)
        {
            _Content = templateProvider;
            return this;
        }

        public PageProviderBuilder Add(IConcernBuilder concern)
        {
            _Concerns.Add(concern);
            return this;
        }

        public IHandler Build(IHandler parent)
        {
            if (_Content == null)
            {
                throw new BuilderMissingPropertyException("Content");
            }

            return Concerns.Chain(parent, _Concerns, (p) => new PageProvider(p, _Info.Build(), _Content));
        }

        #endregion
    }

}
