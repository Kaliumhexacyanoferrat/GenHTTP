﻿using System.IO.Compression;
using GenHTTP.Api.Content;
using GenHTTP.Api.Content.IO;
using GenHTTP.Api.Infrastructure;

namespace GenHTTP.Modules.Compression.Providers;

public sealed class CompressionConcernBuilder : IConcernBuilder
{
    private readonly List<ICompressionAlgorithm> _Algorithms = [];

    private CompressionLevel _Level = CompressionLevel.Fastest;

    #region Functionality

    public CompressionConcernBuilder Add(IBuilder<ICompressionAlgorithm> algorithm) => Add(algorithm.Build());

    public CompressionConcernBuilder Add(ICompressionAlgorithm algorithm)
    {
        _Algorithms.Add(algorithm);
        return this;
    }

    public CompressionConcernBuilder Level(CompressionLevel level)
    {
        _Level = level;
        return this;
    }

    public IConcern Build(IHandler content)
    {
        var algorithms = _Algorithms.ToDictionary(a => a.Name);

        return new CompressionConcern(content, algorithms, _Level);
    }

    #endregion

}
