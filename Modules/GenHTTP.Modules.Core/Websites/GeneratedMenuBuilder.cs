using System;
using System.Collections.Generic;
using System.Text;

using GenHTTP.Api.Infrastructure;
using GenHTTP.Api.Modules.Websites;
using GenHTTP.Api.Routing;

namespace GenHTTP.Modules.Core.Websites
{

    public class GeneratedMenuBuilder : IBuilder<IMenuProvider>
    {
        private IRouter? _Router;

        #region Functionality

        public GeneratedMenuBuilder Router(IRouter router)
        {
            _Router = router;
            return this;
        }

        public IMenuProvider Build()
        {
            var router = _Router ?? throw new BuilderMissingPropertyException("router");

            return new GeneratedMenuProvider(router);
        }

        #endregion

    }

}
