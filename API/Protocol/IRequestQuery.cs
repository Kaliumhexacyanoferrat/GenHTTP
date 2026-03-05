namespace GenHTTP.Api.Protocol;

// todo: re-visit

/// <summary>
/// Stores the query sent by the client.
/// </summary>
public interface IRequestQuery : IReadOnlyDictionary<string, string>;
