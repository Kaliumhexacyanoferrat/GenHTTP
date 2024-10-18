using System.Text;
using GenHTTP.Api.Protocol;
using NSwag;

namespace GenHTTP.Modules.OpenApi.Handler;

internal class OpenApiContent : IResponseContent
{

    #region Initialization

    internal OpenApiContent(ReturnDocument document, OpenApiFormat format)
    {
        Document = document;
        Format = format;
    }

    #endregion

    #region Get-/Setters

    public ulong? Length => null;

    private ReturnDocument Document { get; }

    private OpenApiFormat Format { get; }

    #endregion

    #region Functionality

    public ValueTask<ulong?> CalculateChecksumAsync() => new(Document.Checksum);

    public async ValueTask WriteAsync(Stream target, uint bufferSize)
    {
        if (Format == OpenApiFormat.Json)
        {
            await target.WriteAsync(Encoding.UTF8.GetBytes(Document.Document.ToJson()));
        }
        else
        {
            await target.WriteAsync(Encoding.UTF8.GetBytes(Document.Document.ToYaml()));
        }
    }

    #endregion

}
