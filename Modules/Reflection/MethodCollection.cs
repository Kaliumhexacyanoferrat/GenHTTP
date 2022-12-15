using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using GenHTTP.Api.Content;
using GenHTTP.Api.Protocol;
using GenHTTP.Api.Routing;

namespace GenHTTP.Modules.Reflection
{

    public sealed class MethodCollection : IHandler, IHandlerResolver, IRootPathAppender
    {

        #region Get-/Setters

        public IHandler Parent { get; }

        public List<MethodHandler> Methods { get; }

        #endregion

        #region Initialization

        public MethodCollection(IHandler parent, IEnumerable<Func<IHandler, MethodHandler>> methodFactories)
        {
            Parent = parent;

            Methods = methodFactories.Select(factory => factory(this))
                                     .ToList();
        }

        #endregion

        #region Functionality

        public ValueTask<IResponse?> HandleAsync(IRequest request)
        {
            var methods = FindProviders(request.Target.GetRemaining().ToString(), request.Method, out var foundOthers);

            if (methods.Count == 1)
            {
                return methods[0].HandleAsync(request);
            }
            else if (methods.Count > 1)
            {
                throw new ProviderException(ResponseStatus.BadRequest, $"There are multiple methods matching '{request.Target.Path}'");
            }
            else
            {
                if (foundOthers)
                {
                    throw new ProviderException(ResponseStatus.MethodNotAllowed, $"There is no method of a matching request type");
                }

                return new ValueTask<IResponse?>();
            }
        }

        public async ValueTask PrepareAsync()
        {
            foreach (var handler in Methods)
            {
                await handler.PrepareAsync();
            }
        }

        public async IAsyncEnumerable<ContentElement> GetContentAsync(IRequest request)
        {
            foreach (var method in Methods)
            {
                await foreach (var content in method.GetContentAsync(request))
                {
                    yield return content;
                }
            }
        }

        private List<MethodHandler> FindProviders(string path, FlexibleRequestMethod requestedMethod, out bool foundOthers)
        {
            foundOthers = false;

            var result = new List<MethodHandler>(2);

            foreach (var method in Methods)
            {
                if (method.Routing.IsIndex && path == "/")
                {
                    if (method.Configuration.SupportedMethods.Contains(requestedMethod))
                    {
                        result.Add(method);
                    }
                    else
                    {
                        foundOthers = true;
                    }
                }
                else
                {
                    if (method.Routing.ParsedPath.IsMatch(path))
                    {
                        if (method.Configuration.SupportedMethods.Contains(requestedMethod))
                        {
                            result.Add(method);
                        }
                        else
                        {
                            foundOthers = true;
                        }
                    }
                }
            }

            return result;
        }

        public IHandler? Find(string segment)
        {
            return Methods.FirstOrDefault(m => m.Routing.Segment == segment);
        }

        public void Append(PathBuilder path, IRequest request, IHandler? child = null)
        {
            var handler = Methods.FirstOrDefault(m => m == child);

            if (handler is not null)
            {
                var match = handler.GetMatchedPath(request);

                if (path.IsEmpty)
                {
                    path.TrailingSlash(true);
                }

                if (match is not null)
                {
                    path.Preprend(match);
                }
                else
                {
                    if (handler.Routing.Segment is not null)
                    {
                        path.Preprend(handler.Routing.Segment);
                    }
                }
            }
        }

        #endregion

    }

}
