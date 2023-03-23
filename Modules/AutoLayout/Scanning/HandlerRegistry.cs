using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using GenHTTP.Api.Content;
using GenHTTP.Api.Content.IO;

namespace GenHTTP.Modules.AutoLayout.Scanning
{

    public class HandlerRegistry
    {

        #region Get-/Setters

        public List<IResourceHandlerProvider> Providers { get; }
        
        public IResourceHandlerProvider Fallback { get; }

        #endregion

        #region Initialization

        public HandlerRegistry(List<IResourceHandlerProvider> providers, IResourceHandlerProvider fallback)
        {
            Providers = providers;
            Fallback = fallback;
        }

        #endregion

        #region Functionality

        public async ValueTask<IHandlerBuilder> ResolveAsync(IResource resource)
        {
            foreach (var provider in Providers)
            {
                if (provider.Supports(resource))
                {
                    return await provider.GetHandlerAsync(resource);
                }
            }

            if (!Fallback.Supports(resource))
            {
                throw new InvalidOperationException($"Fallback cannot handle resource '{resource.Name}'");
            }

            return await Fallback.GetHandlerAsync(resource);
        }

        #endregion

    }

}
