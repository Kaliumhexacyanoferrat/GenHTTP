﻿using System;

using GenHTTP.Api.Content;

namespace GenHTTP.Modules.IO.Ranges
{

    public class RangeSupportConcernBuilder : IConcernBuilder
    {

        #region Functionality

        public IConcern Build(IHandler parent, Func<IHandler, IHandler> contentFactory)
        {
            return new RangeSupportConcern(parent, contentFactory);
        }

        #endregion

    }

}
