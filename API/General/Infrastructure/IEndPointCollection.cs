namespace GenHTTP.Api.Infrastructure;

/// <summary>
/// Provides a list of endpoints a server is listening to.
/// </summary>
public interface IEndPointCollection : IReadOnlyList<IEndPoint>;
