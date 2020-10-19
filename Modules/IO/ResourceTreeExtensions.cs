using GenHTTP.Api.Content;
using GenHTTP.Api.Content.IO;
using GenHTTP.Api.Protocol;
using GenHTTP.Api.Routing;
using GenHTTP.Modules.Basics;
using System.Collections.Generic;
using System.Linq;

namespace GenHTTP.Modules.IO
{

    public static class ResourceTreeExtensions
    {

        public static IResource? Find(this IResourceNode node, RoutingTarget target)
        {
            var current = target.Current;

            if (current != null)
            {
                if (target.Last)
                {
                    if (node.GetResources().TryGetValue(current, out var resource))
                    {
                        return resource;
                    }

                    return null;
                }
                else
                {
                    if (node.GetNodes().TryGetValue(current, out var childNode))
                    {
                        target.Advance();
                        return childNode.Find(target);
                    }
                }
            }

            return null;
        }

        public static IEnumerable<ContentElement> GetContent(this IResourceNode node, IRequest request)
        {
            var path = node.GetPath(request);

            foreach (var childNode in node.GetNodes())
            {
                var nodePath = path.Edit(false)
                                   .Append(childNode.Key)
                                   .Build();

                // ToDo: Allow providers (e.g. listing) to influence this entry
                // ToDo: Allow to just have the structure but no actual content

                yield return new ContentElement(nodePath, new ContentInfo(), ContentType.ApplicationForceDownload, childNode.Value.GetContent(request));
            }

            foreach (var resource in node.GetResources())
            {
                var resourcePath = path.Edit(false)
                                       .Append(resource.Key)
                                       .Build();

                // ToDo: Add content information to IResource as well?

                var contentType = resource.Value.ContentType ?? new FlexibleContentType(resource.Key.GuessContentType() ?? ContentType.ApplicationForceDownload);

                yield return new ContentElement(resourcePath, new ContentInfo(), contentType);
            }
        }

        public static WebPath GetPath(this IResourceNode node, IRequest request)
        {
            var segments = new List<string>();

            var current = node;

            while (!current.IsRoot)
            {
                var name = current.GetName();

                if (name != null)
                {
                    segments.Add(name);
                }

                current = current.Parent;
            }

            segments.Reverse();

            var path = new PathBuilder(true).Append(request.Target.Path);

            foreach (var segment in segments)
            {
                path.Append(segment);
            }

            return path.Build();
        }

        public static string? GetName(this IResourceNode node)
        {
            if (!node.IsRoot)
            {
                return node.Parent.GetNodes()
                                  .Where(kv => kv.Value == node)
                                  .Select(kv => kv.Key)
                                  .FirstOrDefault();
            }

            return null;
        }

    }

}
