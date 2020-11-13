using System.Collections.Generic;

using GenHTTP.Api.Infrastructure;
using GenHTTP.Api.Content;

namespace GenHTTP.Modules.Basics.Providers
{

    public class RedirectProviderBuilder : IHandlerBuilder<RedirectProviderBuilder>
    {
        private bool _Temporary = false;

        private string? _Location;

        private readonly List<IConcernBuilder> _Concerns = new();

        #region Functionality

        public RedirectProviderBuilder Location(string location)
        {
            _Location = location;
            return this;
        }

        public RedirectProviderBuilder Mode(bool temporary)
        {
            _Temporary = temporary;
            return this;
        }

        public RedirectProviderBuilder Add(IConcernBuilder concern)
        {
            _Concerns.Add(concern);
            return this;
        }

        public IHandler Build(IHandler parent)
        {
            if (_Location == null)
            {
                throw new BuilderMissingPropertyException("Location");
            }

            return Concerns.Chain(parent, _Concerns, (p) => new RedirectProvider(p, _Location, _Temporary));
        }

        #endregion

    }

}
