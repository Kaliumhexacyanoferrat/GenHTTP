using System.Collections.Generic;

using GenHTTP.Api.Content;
using GenHTTP.Api.Infrastructure;

namespace GenHTTP.Modules.Core.Listing
{

    public class ListingRouterBuilder : IHandlerBuilder<ListingRouterBuilder>
    {
        private string? _Directory;

        private List<IConcernBuilder> _Concerns = new List<IConcernBuilder>();

        #region Functionality

        public ListingRouterBuilder Directory(string directory)
        {
            _Directory = directory;
            return this;
        }

        public ListingRouterBuilder Add(IConcernBuilder concern)
        {
            _Concerns.Add(concern);
            return this;
        }

        public IHandler Build(IHandler parent)
        {
            var directory = _Directory ?? throw new BuilderMissingPropertyException("directory");

            return Concerns.Chain(parent, _Concerns, (p) => new ListingRouter(p, directory));
        }

        #endregion

    }

}
