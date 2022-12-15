using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using GenHTTP.Api.Content;
using GenHTTP.Api.Protocol;

using GenHTTP.Modules.Basics;

namespace GenHTTP.Modules.ClientCaching.Policy
{

    public sealed class CachePolicyConcern : IConcern
    {

        #region Get-/Setters

        public IHandler Content { get; }

        public IHandler Parent { get; }

        private TimeSpan Duration { get; }

        private Func<IRequest, IResponse, bool>? Predicate { get; }

        #endregion

        #region Initialization

        public CachePolicyConcern(IHandler parent, Func<IHandler, IHandler> contentFactory, 
                                  TimeSpan duration, Func<IRequest, IResponse, bool>? predicate)
        {
            Parent = parent;
            Content = contentFactory(this);

            Duration = duration;
            Predicate = predicate;
        }

        #endregion

        #region Functionality

        public async ValueTask<IResponse?> HandleAsync(IRequest request)
        {
            var response = await Content.HandleAsync(request);

            if (response != null)
            {
                if (request.HasType(RequestMethod.GET) && (response.Status.KnownStatus == ResponseStatus.OK))
                {
                    if ((Predicate == null) || Predicate(request, response))
                    {
                        response.Expires = DateTime.UtcNow.Add(Duration);
                    }
                }
            }

            return response;
        }

        public IAsyncEnumerable<ContentElement> GetContentAsync(IRequest request) => Content.GetContentAsync(request);

        public ValueTask PrepareAsync() => Content.PrepareAsync();

        #endregion

    }

}
