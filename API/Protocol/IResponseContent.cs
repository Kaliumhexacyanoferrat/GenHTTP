namespace GenHTTP.Api.Protocol;

public interface IResponseContent
{

    ulong? Length { get; }

    ContentType? Type { get; }

    ReadOnlyMemory<byte>? Encoding { get; }

    ValueTask WriteAsync(IResponseSink sink);

}
