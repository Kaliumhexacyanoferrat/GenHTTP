using GenHTTP.Api.Infrastructure;
using GenHTTP.Api.Content;

namespace GenHTTP.Modules.Core.General
{

    public class RedirectProviderBuilder : IHandlerBuilder
    {
        private bool _Temporary = false;

        private string? _Location;

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

        public IHandler Build(IHandler parent)
        {
            if (_Location == null)
            {
                throw new BuilderMissingPropertyException("Location");
            }

            return new RedirectProvider(parent, _Location, _Temporary);
        }

        #endregion

    }

}
