using System;
using System.Collections.Generic;
using System.Text;

using GenHTTP.Api.Infrastructure;
using GenHTTP.Api.Modules;
using GenHTTP.Api.Routing;

using GenHTTP.Modules.Core.General;

namespace GenHTTP.Modules.Core.Listing
{

    public class ListingRouterBuilder : RouterBuilderBase<ListingRouter>
    {
        private string? _Directory;

        private ResponseModification? _Modification;
        
        #region Functionality

        public ListingRouterBuilder Directory(string directory)
        {
            _Directory = directory;
            return this;
        }

        public ListingRouterBuilder Modify(ResponseModification modification)
        {
            _Modification = modification;
            return this;
        }

        public override IRouter Build()
        {
            var directory = _Directory ?? throw new BuilderMissingPropertyException("directory");

            return new ListingRouter(directory, _Modification, _Template, _ErrorHandler);
        }

        #endregion

    }

}
