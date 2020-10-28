using System.Collections.Generic;

using GenHTTP.Api.Content;
using GenHTTP.Api.Content.IO;
using GenHTTP.Api.Protocol;
using GenHTTP.Api.Routing;

using GenHTTP.Modules.Basics;

namespace GenHTTP.Modules.IO
{

    public static class ResourceTreeExtensions
    {

        public static IResource? Find(this IResourceContainer node, RoutingTarget target)
        {
            var current = target.Current;

            if (current != null)
            {
                if (target.Last)
                {
                    if (node.TryGetResource(current, out var resource))
                    {
                        return resource;
                    }

                    return null;
                }
                else
                {
                    if (node.TryGetNode(current, out var childNode))
                    {
                        target.Advance();
                        return childNode.Find(target);
                    }
                }
            }

            return null;
        }

        public static IEnumerable<ContentElement> GetContent(this IResourceContainer node, IRequest request)
        {
            var path = node.GetPath(request);

            foreach (var childNode in node.GetNodes())
            {
                var nodePath = path.Edit(false)
                                   .Append(childNode.Name)
                                   .Build();

                // ToDo: Allow providers (e.g. listing) to influence this entry
                // ToDo: Allow to just have the structure but no actual content

                yield return new ContentElement(nodePath, new ContentInfo(), ContentType.ApplicationForceDownload, childNode.GetContent(request));
            }

            foreach (var resource in node.GetResources())
            {
                var name = resource.Name;

                if (name != null)
                {
                    var resourcePath = path.Edit(false)
                                           .Append(name)
                                           .Build();

                    // ToDo: Add content information to IResource as well?

                    var contentType = resource.ContentType ?? new FlexibleContentType(name.GuessContentType() ?? ContentType.ApplicationForceDownload);

                    yield return new ContentElement(resourcePath, new ContentInfo(), contentType);
                }
            }
        }

        public static WebPath GetPath(this IResourceContainer node, IRequest request)
        {
            var segments = new List<string>();

            var current = node;

            while (current is IResourceNode currentNode)
            {
                segments.Add(currentNode.Name);

                current = currentNode.Parent;
            }

            segments.Reverse();

            var path = new PathBuilder(true).Append(request.Target.Path);

            foreach (var segment in segments)
            {
                path.Append(segment);
            }

            return path.Build();
        }

    }

}
