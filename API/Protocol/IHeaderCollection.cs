namespace GenHTTP.Api.Protocol;

// todo: re-visit

/// <summary>
/// The headers of an <see cref="IRequest" /> or <see cref="IResponse" />.
/// </summary>
public interface IHeaderCollection : IReadOnlyDictionary<string, string>;
