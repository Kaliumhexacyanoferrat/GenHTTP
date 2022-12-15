using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using GenHTTP.Api.Content;
using GenHTTP.Api.Protocol;
using GenHTTP.Api.Routing;

using GenHTTP.Modules.Basics;

namespace GenHTTP.Modules.Layouting.Provider
{

    public sealed class LayoutRouter : IHandler, IRootPathAppender, IHandlerResolver
    {

        #region Get-/Setters

        public IHandler Parent { get; }

        private Dictionary<string, IHandler> RoutedHandlers { get; }

        private List<IHandler> RootHandlers { get; }

        private IHandler? Index { get; }

        #endregion

        #region Initialization

        public LayoutRouter(IHandler parent,
                            Dictionary<string, IHandlerBuilder> routedHandlers,
                            List<IHandlerBuilder> rootHandlers,
                            IHandlerBuilder? index)
        {
            Parent = parent;

            RoutedHandlers = routedHandlers.ToDictionary(kv => kv.Key, kv => kv.Value.Build(this));

            RootHandlers = rootHandlers.Select(h => h.Build(this)).ToList();

            Index = index?.Build(this);
        }

        #endregion

        #region Functionality

        public async ValueTask<IResponse?> HandleAsync(IRequest request)
        {
            var current = request.Target.Current;

            if (current is not null)
            {
                if (RoutedHandlers.TryGetValue(current.Value, out var handler))
                {
                    request.Target.Advance();

                    return await handler.HandleAsync(request).ConfigureAwait(false);
                }
            }
            else
            {
                // force a trailing slash to prevent duplicate content
                if (!request.Target.Path.TrailingSlash)
                {
                    return await Redirect.To($"{request.Target.Path}/")
                                         .Build(this)
                                         .HandleAsync(request);
                }

                if (Index is not null)
                {
                    return await Index.HandleAsync(request);
                }
            }

            foreach (var handler in RootHandlers)
            {
                var result = await handler.HandleAsync(request);

                if (result != null)
                {
                    return result;
                }
            }

            return null;
        }

        public async ValueTask PrepareAsync()
        {
            if (Index != null)
            {
                await Index.PrepareAsync();
            }

            foreach (var handler in RoutedHandlers.Values)
            {
                await handler.PrepareAsync();
            }

            foreach (var handler in RootHandlers)
            {
                await handler.PrepareAsync();
            }
        }

        public IAsyncEnumerable<ContentElement> GetContentAsync(IRequest request)
        {
            var result = new List<ContentElement>();

            if (Index is not null)
            {
                result.AddRange(Index.GetContentAsync(request).ToEnumerable());
            }

            foreach (var handler in RoutedHandlers.Values)
            {
                result.AddRange(handler.GetContentAsync(request).ToEnumerable());
            }

            foreach (var handler in RootHandlers)
            {
                result.AddRange(handler.GetContentAsync(request).ToEnumerable());
            }

            return result.ToAsyncEnumerable();
        }

        public void Append(PathBuilder path, IRequest request, IHandler? child = null)
        {
            if (child is not null)
            {
                if (child == Index)
                {
                    path.TrailingSlash(true);
                }
                else
                {
                    foreach (var entry in RoutedHandlers)
                    {
                        if (entry.Value == child)
                        {
                            path.Preprend(entry.Key);
                            break;
                        }
                    }
                }
            }
        }

        public IHandler? Find(string segment)
        {
            if (RoutedHandlers.TryGetValue(segment, out var handler))
            {
                return handler;
            }

            if (Index is not null && segment == "{index}")
            {
                return Index;
            }

            // remove as soon as LayoutBuilder.Fallback is removed
            if (segment == "{layout}" || segment == "{fallback}") 
            {
                return this;
            }

            return null;
        }

        #endregion

    }

}
