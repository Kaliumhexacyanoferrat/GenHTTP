﻿using GenHTTP.Api.Content;
using GenHTTP.Api.Protocol;

namespace GenHTTP.Modules.Reflection.Injectors;

public class RequestInjector : IParameterInjector
{

    public bool Supports(IRequest request, Type type) => type == typeof(IRequest);

    public object GetValue(IHandler handler, IRequest request, Type targetType) => request;
}
