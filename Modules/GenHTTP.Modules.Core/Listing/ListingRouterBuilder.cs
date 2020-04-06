using GenHTTP.Api.Infrastructure;
using GenHTTP.Api.Protocol;

namespace GenHTTP.Modules.Core.Listing
{

    public class ListingRouterBuilder : IHandlerBuilder
    {
        private string? _Directory;

        #region Functionality

        public ListingRouterBuilder Directory(string directory)
        {
            _Directory = directory;
            return this;
        }

        public IHandler Build(IHandler parent)
        {
            var directory = _Directory ?? throw new BuilderMissingPropertyException("directory");

            return new ListingRouter(parent, directory);
        }

        #endregion

    }

}
