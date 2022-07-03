using System;

using GenHTTP.Api.Content;
using GenHTTP.Api.Infrastructure;

namespace GenHTTP.Modules.Security.Providers
{

    public sealed class SecurityConcernBuilder : IConcernBuilder
    {
        private XContentTypeOptions? _Options = XContentTypeOptions.NoSniff;

        #region Functionality

        public SecurityConcernBuilder Options(XContentTypeOptions? options)
        {
            _Options = options;
            return this;
        }

        public IConcern Build(IHandler parent, Func<IHandler, IHandler> contentFactory)
        {
            var options = _Options ?? throw new BuilderMissingPropertyException("options");
            return new SecurityConcern(parent, contentFactory, options);
        }

        #endregion

    }

}
