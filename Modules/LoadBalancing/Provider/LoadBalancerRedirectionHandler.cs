using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using GenHTTP.Api.Content;
using GenHTTP.Api.Protocol;

using GenHTTP.Modules.Basics;

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
        
        public ValueTask<IResponse?> HandleAsync(IRequest request)
        {
            return Redirect.To(Root + request.Target.Current, true)
                           .Build(this)
                           .HandleAsync(request);
        }

        public IEnumerable<ContentElement> GetContent(IRequest request) => Enumerable.Empty<ContentElement>();

        #endregion

    }

}
