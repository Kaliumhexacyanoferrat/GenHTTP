using GenHTTP.Api.Content;

namespace GenHTTP.Engine.Shared.Infrastructure.Compliance;

public sealed class ComplianceConcernBuilder : IConcernBuilder
{

    public IConcern Build(IHandler content) => new ComplianceConcern(content);

}
