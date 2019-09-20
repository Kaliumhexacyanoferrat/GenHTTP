using System;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;

using GenHTTP.Api.Modules;
using GenHTTP.Api.Modules.Templating;
using GenHTTP.Api.Routing;
using GenHTTP.Api.Protocol;

using GenHTTP.Modules.Core.General;

namespace GenHTTP.Modules.Webservices
{

    public class ResourceRouter : RouterBase
    {

        #region Get-/Setters

        private Type Type { get; }

        private object Instance { get; }

        private List<MethodProvider> Methods { get; }

        private SerializationRegistry Serialization { get; }

        #endregion

        #region Initialization

        public ResourceRouter(object instance,
                              SerializationRegistry formats,
                              IRenderer<TemplateModel>? template,
                              IContentProvider? errorHandler) : base(template, errorHandler)
        {
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
                    yield return new MethodProvider(method, Instance, attribute, Serialization);
                }
            }
        }

        #endregion

        #region Functionality

        public override void HandleContext(IEditableRoutingContext current)
        {
            current.Scope(this);

            var methods = FindProviders(current.ScopedPath);

            if (methods.Any())
            {
                var matchingMethods = methods.Where(m => current.Request.Method.Equals(m.MetaData.RequestMethod)).ToList();

                if (matchingMethods.Count == 1)
                {
                    current.RegisterContent(matchingMethods.First());
                    return;
                }
                else if (methods.Count > 1)
                {
                    throw new ProviderException(ResponseStatus.BadRequest, $"There are multiple methods matching '{current.Request.Path}'");
                }
                else
                {
                    throw new ProviderException(ResponseStatus.MethodNotAllowed, $"There is no method of a matching request type");
                }
            }
        }

        public override string? Route(string path, int currentDepth)
        {
            if (FindProviders($"/{path}").Any())
            {
                return Api.Routing.Route.GetRelation(currentDepth) + path;
            }

            return Parent.Route(path, currentDepth + 1);
        }

        private List<MethodProvider> FindProviders(string path) => Methods.Where(m => m.ParsedPath?.IsMatch(path) ?? false).ToList();

        #endregion

    }

}
