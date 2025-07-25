﻿using GenHTTP.Api.Content;
using GenHTTP.Api.Protocol;

namespace GenHTTP.Modules.Reflection.Injectors;

public class HandlerInjector : IParameterInjector
{

    public bool Supports(IRequest request, Type type) => type == typeof(IHandler);

    public object GetValue(IHandler handler, IRequest request, Type targetType) => handler;
}
