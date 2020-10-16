using System;

using GenHTTP.Api.Content;

namespace GenHTTP.Modules.ClientCaching.Provider
{

    public class CacheValidationBuilder : IConcernBuilder
    {

        public IConcern Build(IHandler parent, Func<IHandler, IHandler> contentFactory)
        {
            return new CacheValidationHandler(parent, contentFactory);
        }

    }

}
