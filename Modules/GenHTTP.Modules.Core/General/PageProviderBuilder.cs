using System.Collections.Generic;

using GenHTTP.Api.Infrastructure;
using GenHTTP.Api.Content;

namespace GenHTTP.Modules.Core.General
{

    public class PageProviderBuilder : IHandlerBuilder<PageProviderBuilder>
    {
        private IResourceProvider? _Content;
        private string? _Title;

        private List<IConcernBuilder> _Concerns = new List<IConcernBuilder>();

        #region Functionality

        public PageProviderBuilder Title(string title)
        {
            _Title = title;
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

            return Concerns.Chain(parent, _Concerns, (p) => new PageProvider(p, _Title, _Content));
        }

        #endregion

    }

}
