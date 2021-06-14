using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using GenHTTP.Api.Content;
using GenHTTP.Api.Protocol;

namespace GenHTTP.Modules.IO.Ranges
{

    public class RangeSupportConcern : IConcern
    {

        #region Get-/Setters

        public IHandler Content { get; }

        public IHandler Parent { get; }

        #endregion

        #region Initialization

        public RangeSupportConcern(IHandler parent, Func<IHandler, IHandler> contentFactory)
        {
            Parent = parent;
            Content = contentFactory(this);
        }

        #endregion

        #region Functionality

        public ValueTask PrepareAsync() => ValueTask.CompletedTask;

        public IEnumerable<ContentElement> GetContent(IRequest request) => Content.GetContent(request);

        public async ValueTask<IResponse?> HandleAsync(IRequest request)
        {
            // ToDo: Parse range header (if any)

            var response = await Content.HandleAsync(request);

            return response;
        }

        #endregion

    }

}
