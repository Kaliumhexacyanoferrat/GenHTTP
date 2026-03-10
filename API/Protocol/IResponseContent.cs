namespace GenHTTP.Api.Protocol;

public interface IResponseContent
{

    ulong? Length { get; }

    ValueTask WriteAsync(IResponseSink sink);

}
