using GenHTTP.Api.Content;

namespace GenHTTP.Engine.Shared.Infrastructure.Compliance;

public class ComplianceConcernBuilder : IConcernBuilder
{

    public IConcern Build(IHandler content) => new ComplianceConcern(content);

}
