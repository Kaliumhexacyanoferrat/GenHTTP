namespace GenHTTP.Api.Protocol;

public interface IResponseContent
{

    ulong? Length { get; }

    ContentType? Type { get; }

    ReadOnlyMemory<byte>? Encoding { get; }

    ValueTask<ulong?> CalculateChecksumAsync();

    ValueTask WriteAsync(IResponseSink sink);

}
