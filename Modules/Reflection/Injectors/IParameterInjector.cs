using System;

using GenHTTP.Api.Content;
using GenHTTP.Api.Protocol;

namespace GenHTTP.Modules.Reflection.Injectors
{

    public interface IParameterInjector
    {

        bool Supports(Type type);

        object? GetValue(IHandler handler, IRequest request, Type targetType);

    }

}
