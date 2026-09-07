namespace GenHTTP.Api.Protocol;

/// <summary>
/// The query parameters of an incoming HTTP request (see <see cref="IRequestHeader.Query"/>).
/// </summary>
public interface IRequestQuery : IKeyValueList;
