namespace GenHTTP.Api.Protocol;

/// <summary>
/// A collection representing the cookies of an <see cref="IRequest"/>
/// or <see cref="IResponse"/>.
/// </summary>
public interface ICookieCollection : IReadOnlyDictionary<string, Cookie>, IDisposable
{

}
