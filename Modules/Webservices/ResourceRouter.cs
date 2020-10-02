using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

using GenHTTP.Api.Content;
using GenHTTP.Api.Protocol;
using GenHTTP.Api.Routing;

using GenHTTP.Modules.Basics;

namespace GenHTTP.Modules.Webservices
{

    public class ResourceRouter : IHandler
    {

        #region Get-/Setters

        public IHandler Parent { get; }

        private Type Type { get; }

        private object Instance { get; }

        private List<MethodProvider> Methods { get; }

        private SerializationRegistry Serialization { get; }

        #endregion

        #region Initialization

        public ResourceRouter(IHandler parent, object instance, SerializationRegistry formats)
        {
            Parent = parent;

            Instance = instance;
            Type = instance.GetType();

            Serialization = formats;

            Methods = new List<MethodProvider>(AnalyzeMethods());
        }

        private IEnumerable<MethodProvider> AnalyzeMethods()
        {
            foreach (var method in Type.GetMethods(BindingFlags.Public | BindingFlags.Instance))
            {
                var attribute = method.GetCustomAttribute<MethodAttribute>(true);

                if (attribute != null)
                {
                    yield return new MethodProvider(Parent, method, Instance, attribute, Serialization);
                }
            }
        }

        #endregion

        #region Functionality

        public IResponse? Handle(IRequest request)
        {
            var methods = FindProviders(request.Target.GetRemaining().ToString());

            if (methods.Any())
            {
                var matchingMethods = methods.Where(m => request.Method.Equals(m.MetaData.RequestMethod)).ToList();

                if (matchingMethods.Count == 1)
                {
                    return matchingMethods.First().Handle(request);
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

            return null;
        }

        public IEnumerable<ContentElement> GetContent(IRequest request)
        {
            foreach (var method in Methods.Where(m => m.MetaData.RequestMethod == RequestMethod.GET))
            {
                var parts = new List<string>(this.GetRoot(request.Server.Handler, false).Parts);

                WebPath path;

                if (method.MetaData.Path == null)
                {
                    path = new WebPath(parts, true);
                }
                else
                {
                    parts.Add(method.MetaData.Path);
                    path = new WebPath(parts, false);
                }

                var info = ContentInfo.Create()
                                      .Title(Path.GetFileName(path.ToString()))
                                      .Build();

                yield return new ContentElement(path, info, path.ToString().GuessContentType() ?? ContentType.ApplicationForceDownload, null);
            }
        }

        private List<MethodProvider> FindProviders(string path) => Methods.Where(m => m.ParsedPath?.IsMatch(path) ?? false).ToList();

        #endregion

    }

}
