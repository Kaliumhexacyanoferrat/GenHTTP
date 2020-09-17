using System.Collections.Generic;
using System.Linq;

using GenHTTP.Api.Content;
using GenHTTP.Api.Protocol;
using GenHTTP.Modules.Core;

namespace GenHTTP.Modules.LoadBalancing.Provider
{

    public class LoadBalancerRedirectionHandler : IHandler
    {

        #region Get-/Setters

        public IHandler Parent { get; }

        private string Root { get; }

        #endregion

        #region Initialization

        public LoadBalancerRedirectionHandler(IHandler parent, string root)
        {
            Parent = parent;

            Root = root.EndsWith('/') ? root : $"{root}/";
        }

        #endregion

        #region Functionality

        public IResponse? Handle(IRequest request)
        {
            return Redirect.To(Root + request.Target.Current, true)
                           .Build(this)
                           .Handle(request);
        }

        public IEnumerable<ContentElement> GetContent(IRequest request) => Enumerable.Empty<ContentElement>();

        #endregion

    }

}
