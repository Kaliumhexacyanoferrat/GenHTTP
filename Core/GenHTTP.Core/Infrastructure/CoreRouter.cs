using GenHTTP.Api.Content;
using GenHTTP.Api.Protocol;
using System;
using System.Collections.Generic;

namespace GenHTTP.Core.Infrastructure
{

    internal class CoreRouter : IHandler
    {

        #region Get-/Setters

        public IHandler Parent
        {
            get { throw new NotSupportedException("Core router has no parent"); }
            set { throw new NotSupportedException("Setting core router's parent is not allowed"); }
        }

        public IHandler Content { get; }

        #endregion

        #region Initialization

        internal CoreRouter(IHandlerBuilder content)
        {
            Content = content.Build(this);
        }

        #endregion

        #region Functionality

        public IResponse? Handle(IRequest request)
        {
            return Content.Handle(request); // ToDo: 404, exception handling, etc.
        }

        public IEnumerable<ContentElement> GetContent(IRequest request)
        {
            return Content.GetContent(request);
        }

        #endregion

    }

}
