﻿using GenHTTP.Api.Content;
using GenHTTP.Api.Protocol;
using GenHTTP.Api.Routing;
using GenHTTP.Modules.Conversion.Providers;
using GenHTTP.Modules.Reflection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace GenHTTP.Modules.Controllers.Provider
{

    public class ControllerHandler<T> : IHandler, IHandlerResolver where T : new()
    {
        private static readonly MethodRouting EMPTY = new MethodRouting("/", "^(/|)$", null, true);

        #region Get-/Setters

        public IHandler Parent { get; }

        private MethodCollection Provider { get; }

        #endregion

        #region Initialization

        public ControllerHandler(IHandler parent, SerializationRegistry formats)
        {
            Parent = parent;

            Provider = new MethodCollection(this, AnalyzeMethods(typeof(T), formats));
        }

        private IEnumerable<Func<IHandler, MethodHandler>> AnalyzeMethods(Type type, SerializationRegistry formats)
        {
            foreach (var method in type.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly))
            {
                var annotation = method.GetCustomAttribute<ControllerActionAttribute>(true) ?? new MethodAttribute();

                var arguments = FindPathArguments(method);

                var path = DeterminePath(method, arguments);

                yield return (parent) => new MethodHandler(parent, method, path, () => new T(), annotation, GetResponse, formats);
            }
        }

        private MethodRouting DeterminePath(MethodInfo method, List<string> arguments)
        {
            var pathArgs = string.Join('/', arguments.Select(a => a.ToParameter()));
            var rawArgs = string.Join('/', arguments.Select(a => $":{a}"));

            if (method.Name == "Index")
            {
                return pathArgs.Length > 0 ? new MethodRouting($"/{rawArgs}/", $"^/{pathArgs}/", null, false) : EMPTY;
            }
            else
            {
                var name = HypenCase(method.Name);

                var path = $"^/{name}";

                return pathArgs.Length > 0 ? new MethodRouting($"/{name}/{rawArgs}/", $"{path}/{pathArgs}/", name, false) : new MethodRouting($"/{name}", $"{path}/", name, false);
            }
        }

        private List<string> FindPathArguments(MethodInfo method)
        {
            var found = new List<string>();

            var parameters = method.GetParameters();

            foreach (var parameter in parameters)
            {
                if (parameter.GetCustomAttribute(typeof(FromPathAttribute), true) != null)
                {
                    if (!parameter.CheckSimple())
                    {
                        throw new InvalidOperationException("Parameters marked as 'FromPath' must be of a simple type (e.g. string or int)");
                    }

                    if (parameter.CheckNullable())
                    {
                        throw new InvalidOperationException("Parameters marked as 'FromPath' are not allowed to be nullable");
                    }

                    if (parameter.Name == null)
                    {
                        throw new InvalidOperationException("Parameters marked as 'FromPath' must have a name");
                    }

                    found.Add(parameter.Name);
                }
            }

            return found;
        }

        private static string HypenCase(string input)
        {
            return Regex.Replace(input, @"([a-z])([A-Z0-9]+)", "$1-$2").ToLowerInvariant();
        }

        #endregion

        #region Functionality

        public IEnumerable<ContentElement> GetContent(IRequest request) => Provider.GetContent(request);

        public ValueTask<IResponse?> HandleAsync(IRequest request) => Provider.HandleAsync(request);

        private async ValueTask<IResponse?> GetResponse(IRequest request, IHandler methodProvider, object? result)
        {
            if (result == null)
            {
                return request.Respond()
                              .Status(ResponseStatus.NoContent)
                              .Build();
            }

            if (result is IHandlerBuilder handlerBuilder)
            {
                return await handlerBuilder.Build(methodProvider)
                                           .HandleAsync(request)
                                           .ConfigureAwait(false);
            }

            if (result is IHandler handler)
            {
                return await handler.HandleAsync(request)
                                    .ConfigureAwait(false);
            }

            if (result is IResponseBuilder responseBuilder)
            {
                return responseBuilder.Build();
            }
            else if (result is ValueTask<IResponseBuilder> responseBuilderTask)
            {
                return (await responseBuilderTask.ConfigureAwait(false)).Build();
            }
            else if (result is ValueTask<IResponseBuilder?> optionalResponseBuilderTask)
            {
                return (await optionalResponseBuilderTask.ConfigureAwait(false))?.Build();
            }

            if (result is IResponse response)
            {
                return response;
            }
            else if (result is ValueTask<IResponse> responseTask)
            {
                return await responseTask.ConfigureAwait(false);
            }
            else if (result is ValueTask<IResponse?> optionalResponseTask)
            {
                return await optionalResponseTask.ConfigureAwait(false);
            }

            throw new ProviderException(ResponseStatus.InternalServerError, "Result type of controller methods must be one of: IHandlerBuilder, IHandler, IResponseBuilder, IResponse");
        }

        public IHandler? Find(string segment)
        {
            if (segment == "{controller}")
            {
                return this;
            }

            if (segment == "{index}")
            {
                return Provider.Methods.FirstOrDefault(m => m.Method.Name == "Index" && m.MetaData.SupportedMethods.Contains(new FlexibleRequestMethod(RequestMethod.GET)));
            }

            return null;
        }


        #endregion

    }

}
