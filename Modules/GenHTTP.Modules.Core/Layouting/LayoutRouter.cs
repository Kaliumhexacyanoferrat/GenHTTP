using System.Collections.Generic;
using System.Linq;

using GenHTTP.Api.Content;
using GenHTTP.Api.Content.Templating;
using GenHTTP.Api.Protocol;

namespace GenHTTP.Modules.Core.Layouting
{

    public class LayoutRouter : IHandler
    {

        #region Get-/Setters

        public IHandler Parent { get; }

        private Dictionary<string, IHandler> Folders { get; }

        private Dictionary<string, IHandler> Files { get; }

        private IHandler? Index { get; }

        private IHandler? Fallback { get; }

        #endregion

        #region Initialization

        public LayoutRouter(IHandler parent,
                            Dictionary<string, IHandlerBuilder> folders,
                            Dictionary<string, IHandlerBuilder> files,
                            IHandlerBuilder? index,
                            IHandlerBuilder? fallback)
        {
            Parent = parent;

            Folders = folders.ToDictionary(kv => kv.Key, kv => kv.Value.Build(this));
            Files = files.ToDictionary(kv => kv.Key, kv => kv.Value.Build(this));

            Index = index?.Build(this);
            Fallback = fallback?.Build(this);
        }

        #endregion

        #region Functionality

        public IResponse? Handle(IRequest request)
        {
            /*var segment = Api.Routing.Route.GetSegment(current.ScopedPath);

            // is there a matching content provider?
            if (Content.ContainsKey(segment))
            {
                current.Scope(this, segment);
                current.RegisterContent(Content[segment]);
                return;
            }

            // are there any matching routes? 
            if (Routes.ContainsKey(segment))
            {
                current.Scope(this, segment);
                Routes[segment].HandleContext(current);
                return;
            }

            // route by default
            if (string.IsNullOrEmpty(segment) && DefaultContent != null)
            {
                current.Scope(this);
                current.RegisterContent(DefaultContent);
                return;
            }
            else if (DefaultRouter != null)
            {
                current.Scope(this);
                DefaultRouter.HandleContext(current);
                return;
            }

            // no route found
            current.Scope(this);*/

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
