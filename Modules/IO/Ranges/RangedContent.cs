using GenHTTP.Api.Protocol;

namespace GenHTTP.Modules.IO.Ranges;

public class RangedContent : IResponseContent
{

    #region Initialization

    public RangedContent(IResponseContent source, ulong start, ulong end)
    {
        Source = source;

        Start = start;
        End = end;

        Length = End - Start + 1;
    }

    #endregion

    #region Get-/Setters

    public ulong? Length { get; }

    public ContentType? Type => Source.Type;

    public ReadOnlyMemory<byte>? Encoding => Source.Encoding;

    private IResponseContent Source { get; }

    private ulong Start { get; }

    private ulong End { get; }

    #endregion

    #region Functionality

    /*public async ValueTask<ulong?> CalculateChecksumAsync()
    {
        var checksum = await Source.CalculateChecksumAsync();

        if (checksum != null)
        {
            unchecked
            {
                checksum = checksum * 17 + Start;
                checksum = checksum * 17 + End;
            }
        }

        return checksum;
    }*/

    public ValueTask WriteAsync(IResponseSink sink) => Source.WriteAsync(new RangedSink(new RangedStream(sink.Stream, Start, End)));

    #endregion

}
