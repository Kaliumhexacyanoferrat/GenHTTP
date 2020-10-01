using System.Collections.Generic;

using GenHTTP.Api.Infrastructure;
using GenHTTP.Api.Content;

namespace GenHTTP.Modules.Placeholders.Providers
{

    public class PageProviderBuilder : IHandlerBuilder<PageProviderBuilder>
    {
        private IResourceProvider? _Content;
        private string? _Title;
        private string? _Description;

        private readonly List<IConcernBuilder> _Concerns = new List<IConcernBuilder>();

        #region Functionality

        public PageProviderBuilder Title(string title)
        {
            _Title = title;
            return this;
        }

        public PageProviderBuilder Description(string description)
        {
            _Description = description;
            return this;
        }

        public PageProviderBuilder Content(IResourceProvider templateProvider)
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

            return Concerns.Chain(parent, _Concerns, (p) => new PageProvider(p, _Title, _Description, _Content));
        }

        #endregion
    }

}
