using System;
using System.IO;

using GenHTTP.Api.Content;
using GenHTTP.Api.Protocol;

namespace GenHTTP.Modules.Reflection.Injectors
{

    public class RequestBodyInjector : IParameterInjector
    {

        public bool Supports(Type type) => type == typeof(Stream);

        public object? GetValue(IHandler handler, IRequest request, Type targetType)
        {
            if (request.Content is null)
            {
                throw new ProviderException(ResponseStatus.BadRequest, "Request body expected");
            }

            return request.Content;
        }

    }

}
