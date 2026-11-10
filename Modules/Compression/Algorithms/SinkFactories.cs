using System.IO.Compression;

using GenHTTP.Api.Protocol;

namespace GenHTTP.Modules.Compression.Algorithms;

/// <summary>
/// Creates the sink factory of an algorithm once per compression level,
/// so that compressing a response does not allocate a new delegate.
/// </summary>
internal sealed class SinkFactories
{
    private readonly Func<CompressionLevel, Func<IResponseSink, IResponseSink>> _create;

    private readonly Func<IResponseSink, IResponseSink>[] _byLevel;

    internal SinkFactories(Func<CompressionLevel, Func<IResponseSink, IResponseSink>> create)
    {
        _create = create;

        var levels = Enum.GetValues<CompressionLevel>();

        _byLevel = new Func<IResponseSink, IResponseSink>[(int)levels.Max() + 1];

        for (var i = 0; i < _byLevel.Length; i++)
        {
            _byLevel[i] = create((CompressionLevel)i);
        }
    }

    internal Func<IResponseSink, IResponseSink> Get(CompressionLevel level)
        => (uint)level < (uint)_byLevel.Length ? _byLevel[(int)level] : _create(level);

}
