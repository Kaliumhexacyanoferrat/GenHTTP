﻿using GenHTTP.Api.Content;

namespace GenHTTP.Testing.Acceptance.Utilities;

public class HandlerBuilder : IHandlerBuilder<HandlerBuilder>
{
    private readonly List<IConcernBuilder> _Concerns = [];

    private readonly IHandler _Handler;

    public HandlerBuilder(IHandler handler) { _Handler = handler; }

    public HandlerBuilder Add(IConcernBuilder concern)
    {
        _Concerns.Add(concern);
        return this;
    }

    public IHandler Build()
    {
        return Concerns.Chain(_Concerns, _Handler);
    }

}
