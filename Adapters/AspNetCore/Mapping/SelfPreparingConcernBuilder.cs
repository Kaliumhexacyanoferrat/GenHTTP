using GenHTTP.Api.Content;

namespace GenHTTP.Adapters.AspNetCore.Mapping;

public class SelfPreparingConcernBuilder : IConcernBuilder
{

    public IConcern Build(IHandler content) => new SelfPreparingConcern(content);
    
}
