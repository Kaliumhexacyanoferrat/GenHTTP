using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using GenHTTP.Api.Content;
using GenHTTP.Api.Protocol;

namespace GenHTTP.Modules.Minification.Concern
{

    public sealed class MinifyConcern : IConcern
    {

        #region Get-/Setters

        public IHandler Content { get; }

        public IHandler Parent { get; }

        public IReadOnlyList<IMinificationPlugin> Plugins { get; }

        public MinificationErrors ErrorHandling { get; }

        #endregion

        #region Initialization

        public MinifyConcern(IHandler parent, Func<IHandler, IHandler> contentFactory, IReadOnlyList<IMinificationPlugin> plugins, MinificationErrors errorHandling)
        {
            Parent = parent;
            Content = contentFactory(this);

            Plugins = plugins;
            ErrorHandling = errorHandling;
        }

        #endregion

        #region Functionality

        public ValueTask PrepareAsync() => Content.PrepareAsync();

        public IAsyncEnumerable<ContentElement> GetContentAsync(IRequest request) => Content.GetContentAsync(request);

        public async ValueTask<IResponse?> HandleAsync(IRequest request)
        {
            if (!request.Server.Development)
            {
                var response = await Content.HandleAsync(request);

                if (response != null)
                {
                    foreach (var plugin in Plugins)
                    {
                        if (plugin.Supports(response))
                        {
                            plugin.Process(response, ErrorHandling);
                            break;
                        }
                    }
                }

                return response;
            }
            else
            {
                return await Content.HandleAsync(request);
            }
        }

        #endregion

    }

}
