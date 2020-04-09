using System.Collections.Generic;
using System.Linq;

using GenHTTP.Api.Content;
using GenHTTP.Api.Protocol;

namespace GenHTTP.Modules.Core.Layouting
{

    public class LayoutRouter : IHandler
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
                    // todo
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
            throw new System.NotImplementedException();
        }

        /*public override IEnumerable<ContentElement> GetContent(IRequest request, string basePath)
        {
            foreach (var route in Routes)
            {
                var childPath = $"{basePath}{route.Key}/";

                foreach (var child in route.Value.GetContent(request, childPath))
                {
                    yield return child;
                }
            }

            foreach (var content in Content)
            {
                if (content.Value != DefaultContent)
                {
                    var childPath = $"{basePath}{content.Key}";

                    yield return new ContentElement(childPath, content.Value.Title ?? content.Key, content.Value.ContentType, null);
                }
            }

            if (DefaultRouter != null)
            {
                foreach (var content in DefaultRouter.GetContent(request, basePath))
                {
                    yield return content;
                }
            }

            if (DefaultContent != null)
            {
                yield return new ContentElement(basePath, DefaultContent?.Title ?? "Index", DefaultContent?.ContentType, null);
            }
        }*/

        /*public override string? Route(string path, int currentDepth)
        {
            var segment = Api.Routing.Route.GetSegment(path);

            if (Content.ContainsKey(segment) || Routes.ContainsKey(segment))
            {
                var indexRoute = Content.FirstOrDefault(kv => kv.Key == segment);

                if ((DefaultContent != null) && (DefaultContent == indexRoute.Value))
                {
                    return Api.Routing.Route.GetRelation(currentDepth);
                }

                return Api.Routing.Route.GetRelation(currentDepth) + path;
            }

            return Parent.Route(path, currentDepth + 1);
        }*/

        #endregion

    }

}
