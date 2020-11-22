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

        private Dictionary<string, IHandler> Handlers { get; }

        private IHandler? Index { get; }

        private IHandler? Fallback { get; }

        #endregion

        #region Initialization

        public LayoutRouter(IHandler parent,
                            Dictionary<string, IHandlerBuilder> handlers,
                            IHandlerBuilder? index,
                            IHandlerBuilder? fallback)
        {
            Parent = parent;

            Handlers = handlers.ToDictionary(kv => kv.Key, kv => kv.Value.Build(this));

            Index = index?.Build(this);
            Fallback = fallback?.Build(this);
        }

        #endregion

        #region Functionality

        public ValueTask<IResponse?> HandleAsync(IRequest request)
        {
            var current = request.Target.Current;

            if (current is not null)
            {
                if (Handlers.ContainsKey(current))
                {
                    request.Target.Advance();

                    return Handlers[current].HandleAsync(request);
                }
            }
            else
            {
                // force a trailing slash to prevent duplicate content
                if (!request.Target.Path.TrailingSlash)
                {
                    return Redirect.To($"{request.Target.Path}/")
                                   .Build(this)
                                   .HandleAsync(request);
                }

                if (Index is not null)
                {
                    return Index.HandleAsync(request);
                }
            }

            if (Fallback is not null)
            {
                return Fallback.HandleAsync(request);
            }

            return new ValueTask<IResponse?>();
        }

        public IEnumerable<ContentElement> GetContent(IRequest request)
        {
            var result = new List<ContentElement>();

            if (Index is not null)
            {
                result.AddRange(Index.GetContent(request));
            }

            if (Fallback is not null)
            {
                result.AddRange(Fallback.GetContent(request));
            }

            foreach (var handler in Handlers.Values)
            {
                result.AddRange(handler.GetContent(request));
            }

            return result;
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
                    foreach (var entry in Handlers)
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
            if (Handlers.ContainsKey(segment))
            {
                return Handlers[segment];
            }

            if (Index is not null && segment == "{index}")
            {
                return Index;
            }

            if (Fallback is not null && segment == "{fallback}")
            {
                return Fallback;
            }

            return null;
        }

        #endregion

    }

}
