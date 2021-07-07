using GenHTTP.Api.Content;
using GenHTTP.Modules.IO.Ranges;

namespace GenHTTP.Modules.IO
{

    public static class RangeSupport
    {

        public static RangeSupportConcernBuilder Create() => new();

        #region Extensions

        public static T AddRangeSupport<T>(this T builder) where T : IHandlerBuilder<T>
        {
            builder.AddRangeSupport();
            return builder;
        }

        #endregion

    }

}
