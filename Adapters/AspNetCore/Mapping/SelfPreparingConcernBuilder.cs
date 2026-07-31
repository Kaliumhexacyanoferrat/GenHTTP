using GenHTTP.Api.Content;

namespace GenHTTP.Adapters.AspNetCore.Mapping;

public sealed class SelfPreparingConcernBuilder : IConcernBuilder
{

    public IConcern Build(IHandler content) => new SelfPreparingConcern(content);
    
}
