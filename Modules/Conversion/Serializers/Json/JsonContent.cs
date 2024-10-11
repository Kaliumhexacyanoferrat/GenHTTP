﻿using System.Text.Json;
using GenHTTP.Api.Protocol;

namespace GenHTTP.Modules.Conversion.Serializers.Json;

public sealed class JsonContent : IResponseContent
{

    #region Get-/Setters

    public ulong? Length => null;

    private object Data { get; }

    private JsonSerializerOptions Options { get; }

    #endregion

    #region Initialization

    public JsonContent(object data, JsonSerializerOptions options)
    {
            Data = data;
            Options = options;
        }

    #endregion

    #region Functionality

    public ValueTask<ulong?> CalculateChecksumAsync()
    {
            return new ValueTask<ulong?>((ulong)Data.GetHashCode());
        }

    public async ValueTask WriteAsync(Stream target, uint bufferSize)
    {
            await JsonSerializer.SerializeAsync(target, Data, Data.GetType(), Options);
        }

    #endregion

}
