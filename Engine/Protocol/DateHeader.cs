namespace GenHTTP.Engine.Protocol;

/// <summary>
/// Caches the value of the date header for one second
/// before creating a new value, saving some allocations.
/// </summary>
public static class DateHeader
{
    private static string _value = string.Empty;

    private static byte _second = 61;

    #region Functionality

    public static string GetValue()
    {
        var now = DateTime.UtcNow;

        var second = now.Second;

        if (second != _second)
        {
            _second = (byte)second;
            _value = now.ToString("r");
        }

        return _value;
    }

    #endregion

}
