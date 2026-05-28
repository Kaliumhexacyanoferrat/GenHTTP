using System.Text;
using GenHTTP.Api.Content;
using GenHTTP.Engine.Internal.Context;
using Glyph11.Parser;
using Glyph11.Parser.UltraHardened;

namespace GenHTTP.Benchmarks.Infrastructure;

public sealed class BenchmarkContext
{
    private static readonly ParserLimits Limits = ParserLimits.Default;

    private readonly ClientContext _context;

    private ReadOnlyMemory<byte> _request;

    private readonly MemoryStream _stream = new();

    public BenchmarkContext(string request, IHandler handler)
    {
        _request = Encoding.ASCII.GetBytes(request);

        _context = new ClientContext();

        _context.Apply(new BenchmarkServer(handler), _stream);
    }

    public async ValueTask Execute()
    {
        if (!UltraHardenedParser.TryExtractFullHeaderROM(ref _request, _context.Request.Source, Limits, out _))
        {
            throw new ArgumentException("Given request could not be parsed.");
        }

        _context.Request.Apply(_context.Server);

        await _context.ClientHandler.HandleRequestAsync(_context.Request);

        await _context.Writer.FlushAsync();

        _context.Request.Reset();
        
        _stream.Seek(0, SeekOrigin.Begin);
    }

}
