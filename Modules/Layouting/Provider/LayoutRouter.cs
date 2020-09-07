using System.Collections.Generic;
using System.Linq;

using GenHTTP.Api.Content;
using GenHTTP.Api.Protocol;
using GenHTTP.Api.Routing;

using GenHTTP.Modules.Basics;

namespace GenHTTP.Modules.Layouting.Provider
{

    public class LayoutRouter : IHandler, IRootPathAppender, IHandlerResolver
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

        public IResponse? Handle(IRequest request)
        {
            var current = request.Target.Current;

            if (current != null)
            {
                if (Handlers.ContainsKey(current))
                {
                    request.Target.Advance();
                    return Handlers[current].Handle(request);
                }
            }
            else
            {
                // force a trailing slash to prevent duplicate content
                if (!request.Target.Path.TrailingSlash)
                {
                    return Redirect.To($"{request.Target.Path}/")
                                   .Build(this)
                                   .Handle(request);
                }

                if (Index != null)
                {
                    return Index.Handle(request);
                }
            }

            if (Fallback != null)
            {
                return Fallback.Handle(request);
            }

            return null;
        }

        public IEnumerable<ContentElement> GetContent(IRequest request)
        {
            var result = new List<ContentElement>();

            if (Index != null)
            {
                result.AddRange(Index.GetContent(request));
            }

            if (Fallback != null)
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
            if (child != null)
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

            if (Index != null && segment == "{index}")
            {
                return Index;
            }

            if (Fallback != null && segment == "{fallback}")
            {
                return Fallback;
            }

            return null;
        }

        #endregion

    }

}
