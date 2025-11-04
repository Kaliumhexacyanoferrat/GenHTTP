using GenHTTP.Api.Content;

using GenHTTP.Modules.IO.Ranges;

namespace GenHTTP.Modules.IO;

public static class RangeSupport
{

    public static RangeSupportConcernBuilder Create() => new();

    #region Extensions

    /// <summary>
    /// Adds range support as a concern to the given builder.
    /// </summary>
    public static T AddRangeSupport<T>(this T builder) where T : IHandlerBuilder<T>
    {
        builder.Add(Create());
        return builder;
    }

    #endregion

}
