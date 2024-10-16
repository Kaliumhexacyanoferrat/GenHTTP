using System.Text;

using GenHTTP.Api.Protocol;

using NSwag;

namespace GenHTTP.Modules.OpenApi.Handler;

public class OpenApiContent : IResponseContent
{

    #region Get-/Setters

    public ulong? Length => null;

    private OpenApiDocument Document { get; }

    private OpenApiFormat Format { get; }

    #endregion

    #region Initialization

    public OpenApiContent(OpenApiDocument document, OpenApiFormat format)
    {
        Document = document;
        Format = format;
    }

    #endregion

    #region Functionality

    public ValueTask<ulong?> CalculateChecksumAsync() => new((ulong)Document.ToJson().GetHashCode());

    public async ValueTask WriteAsync(Stream target, uint bufferSize)
    {
        if (Format == OpenApiFormat.Json)
        {
            await target.WriteAsync(Encoding.UTF8.GetBytes(Document.ToJson()));
        }
        else
        {
            await target.WriteAsync(Encoding.UTF8.GetBytes(Document.ToYaml()));
        }
    }

    #endregion

}
