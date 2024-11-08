namespace GenHTTP.Engine.Internal.Utilities;

/// <summary>
/// Caches the string representation of small numbers,
/// reducing the amount of string allocations needed
/// by the engine when writing HTTP responses.
/// </summary>
public static class NumberStringCache
{
    private const int Limit = 1024;

    private static readonly Dictionary<ulong, string> Cache = new
    (
        Enumerable.Range(0, Limit + 1).Select(i => new KeyValuePair<ulong, string>((ulong)i, $"{i}"))
    );

    #region Functionality

    public static string Convert(int number)
    {
        if (number < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(number), "Only positive numbers are supported");
        }

        return Convert((ulong)number);
    }

    public static string Convert(ulong number) => number <= Limit ? Cache[number] : $"{number}";

    #endregion

}
