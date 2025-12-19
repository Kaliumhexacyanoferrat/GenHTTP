namespace GenHTTP.Modules.Conversion.Serializers;

public static class ByteStreamSerialization
{

    /// <summary>
    /// Allows consumers to write a serialized object representation to a memory stream
    /// that will then be converted into a read only memory.
    /// </summary>
    /// <param name="writer">The logic that will perform the serialization</param>
    /// <returns>The byte representation of the serialized data</returns>
    public static async ValueTask<ReadOnlyMemory<byte>> SerializeAsync(Func<MemoryStream, ValueTask> writer)
    {
        using var buffer = new MemoryStream();

        await writer(buffer);

        buffer.Seek(0, SeekOrigin.Begin);

        return buffer.ToArray();
    }

}
