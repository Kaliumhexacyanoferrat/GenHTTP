namespace GenHTTP.Api.Protocol;

public interface IRequestBody
{

    ContentType? Type { get; }

    ReadOnlyMemory<byte>? Encoding { get; }

    ulong? Length { get; }

    Stream AsStream();

    ValueTask<ReadOnlyMemory<byte>> AsMemoryAsync();

}
