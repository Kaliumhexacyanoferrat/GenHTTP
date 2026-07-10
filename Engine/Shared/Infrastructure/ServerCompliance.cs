using GenHTTP.Engine.Shared.Infrastructure.Compliance;

namespace GenHTTP.Engine.Shared.Infrastructure;

public static class ServerCompliance
{

    /// <summary>
    /// Creates a concern that will do additional checks that are
    /// not done by the parser or the engine itself.
    /// </summary>
    /// <returns>The newly created concern instance</returns>
    public static ComplianceConcernBuilder Create() => new();

}
