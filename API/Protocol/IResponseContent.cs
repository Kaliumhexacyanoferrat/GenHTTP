namespace GenHTTP.Api.Protocol;

public interface IResponseContent
{

    ulong? Length { get; }

    ContentType Type { get; }

    ValueTask WriteAsync(IResponseSink sink);

}
