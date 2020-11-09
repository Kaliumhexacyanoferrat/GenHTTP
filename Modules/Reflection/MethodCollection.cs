using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using GenHTTP.Api.Content;
using GenHTTP.Api.Protocol;
using GenHTTP.Api.Routing;

namespace GenHTTP.Modules.Reflection
{

    public class MethodCollection : IHandler, IHandlerResolver, IRootPathAppender
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
            var methods = FindProviders(request.Target.GetRemaining().ToString(), request.Method);

            if (methods.Count == 1)
            {
                return methods[0].HandleAsync(request);
            }
            else if (methods.Count == 0)
            {
                return new ValueTask<IResponse?>();
            }
            else if (methods.Count > 1)
            {
                throw new ProviderException(ResponseStatus.BadRequest, $"There are multiple methods matching '{request.Target.Path}'");
            }
            else
            {
                throw new ProviderException(ResponseStatus.MethodNotAllowed, $"There is no method of a matching request type");
            }
        }

        public IEnumerable<ContentElement> GetContent(IRequest request)
        {
            return Methods.SelectMany(m => m.GetContent(request));
        }

        private List<MethodHandler> FindProviders(string path, FlexibleRequestMethod requestedMethod)
        {
            var result = new List<MethodHandler>(2);

            foreach (var method in Methods)
            {
                if (method.MetaData.SupportedMethods.Contains(requestedMethod))
                {
                    if (method.Routing.IsIndex && path == "/")
                    {
                        result.Add(method);
                    }
                    else
                    {
                        if (method.Routing.ParsedPath.IsMatch(path))
                        {
                            result.Add(method);
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

            if (handler != null)
            {
                var match = handler.GetMatchedPath(request);

                if (path.IsEmpty)
                {
                    path.TrailingSlash(true);
                }

                if (match != null)
                {
                    path.Preprend(match);
                }
                else
                {
                    if (handler.Routing.Segment != null)
                    {
                        path.Preprend(handler.Routing.Segment);
                    }
                }
            }
        }

        #endregion

    }

}
