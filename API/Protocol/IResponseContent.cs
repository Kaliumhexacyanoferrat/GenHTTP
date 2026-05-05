namespace GenHTTP.Api.Protocol;

public interface IResponseContent
{

    ulong? Length { get; }

    ReadOnlyMemory<byte> Type { get; }

    ValueTask WriteAsync(IResponseSink sink);

}
