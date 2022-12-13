using System;

using GenHTTP.Api.Content;

namespace GenHTTP.Modules.Security.Providers
{

    public class SnifferPreventionConcernBuilder : IConcernBuilder
    {

        #region Functionality

        public IConcern Build(IHandler parent, Func<IHandler, IHandler> contentFactory)
        {
            return new SnifferPreventionConcern(parent, contentFactory);
        }

        #endregion

    }

}
