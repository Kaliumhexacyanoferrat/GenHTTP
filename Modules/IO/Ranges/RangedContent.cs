using GenHTTP.Api.Protocol;

namespace GenHTTP.Modules.IO.Ranges;

public class RangedContent : IResponseContent
{

    #region Get-/Setters

    public ulong? Length { get; }

    private IResponseContent Source { get; }

    private ulong Start { get; }

    private ulong End { get; }

    #endregion

    #region Initialization

    public RangedContent(IResponseContent source, ulong start, ulong end)
    {
            Source = source;

            Start = start;
            End = end;

            Length = (End - Start);
        }

    #endregion

    #region Functionality

    public async ValueTask<ulong?> CalculateChecksumAsync()
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
        }

    public ValueTask WriteAsync(Stream target, uint bufferSize) => Source.WriteAsync(new RangedStream(target, Start, End), bufferSize);

    #endregion

}
