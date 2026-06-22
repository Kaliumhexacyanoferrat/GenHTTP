using GenHTTP.Api.Content;

namespace GenHTTP.Engine.Shared.Infrastructure.Logging;

public sealed class RequestLoggingConcernBuilder : IConcernBuilder
{

    public IConcern Build(IHandler content) => new RequestLoggingConcern(content);

}
