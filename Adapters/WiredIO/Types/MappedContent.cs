using System.IO.Pipelines;

using GenHTTP.Api.Protocol;

using Wired.IO.Http11Express.Response.Content;

namespace GenHTTP.Adapters.WiredIO.Types;

public class MappedContent(IResponse source) : IExpressResponseContent
{

    public ulong? Length => source.ContentLength;

    public void Write(PipeWriter writer)
    {
        if (source.Content != null)
        {
            // todo: this is bad
            using var stream = writer.AsStream();

            source.Content.WriteAsync(stream, 4096).AsTask().GetAwaiter().GetResult();

            writer.FlushAsync().AsTask().GetAwaiter().GetResult();
        }
    }

}
