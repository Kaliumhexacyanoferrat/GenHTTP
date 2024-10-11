namespace GenHTTP.Api.Protocol;

/// <summary>
/// Stores the query sent by the client.
/// </summary>
public interface IRequestQuery : IReadOnlyDictionary<string, string>, IDisposable
{

}
