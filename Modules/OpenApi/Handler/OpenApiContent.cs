using GenHTTP.Api.Protocol;

using Microsoft.OpenApi;
using Microsoft.OpenApi.Extensions;
using Microsoft.OpenApi.Models;

namespace GenHTTP.Modules.OpenApi.Handler;

public class OpenApiContent : IResponseContent
{

    #region Get-/Setters

    public ulong? Length => null;

    private OpenApiDocument Document { get; }

    private OpenApiFormat Format { get; }

    private OpenApiSpecVersion Version { get; }

    #endregion

    #region Initialization

    public OpenApiContent(OpenApiDocument document, OpenApiFormat format, OpenApiSpecVersion version)
    {
        Document = document;
        Format = format;
        Version = version;
    }

    #endregion

    #region Functionality

    public ValueTask<ulong?> CalculateChecksumAsync() => new((ulong)Document.HashCode.GetHashCode());

    public ValueTask WriteAsync(Stream target, uint bufferSize)
    {
        Document.Serialize(target, Version, Format);
        return new();
    }

    #endregion

}
