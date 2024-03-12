using GenHTTP.Modules.Conversion.Formatters;

namespace GenHTTP.Modules.Conversion
{

    /// <summary>
    /// Entry point to customize the string representation generated for
    /// various types that can be returned or read by services.
    /// </summary>
    public static class Formatting
    {

        /// <summary>
        /// The default formatters to be used with support for enums, GUIDs,
        /// primitive types and strings.
        /// </summary>
        /// <returns>The default formatters</returns>
        public static FormatterBuilder Default()
        {
            return new FormatterBuilder().Add<StringFormatter>()
                                         .Add<BoolFormatter>()
                                         .Add<EnumFormatter>()
                                         .Add<GuidFormatter>()
                                         .Add<DateOnlyFormatter>()
                                         .Add<PrimitiveFormatter>();
        }

        /// <summary>
        /// Creates an empty formatter registry that can be extended
        /// as needed.
        /// </summary>
        /// <returns>An empty formatter registry</returns>
        public static FormatterBuilder Empty() => new FormatterBuilder();

    }

}
