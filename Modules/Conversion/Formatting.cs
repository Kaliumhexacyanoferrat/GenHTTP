using GenHTTP.Modules.Conversion.Formatters;

namespace GenHTTP.Modules.Conversion
{

    public static class Formatting
    {

        public static FormatterBuilder Default()
        {
            return new FormatterBuilder().Add<StringFormatter>()
                                         .Add<BoolFormatter>()
                                         .Add<EnumFormatter>()
                                         .Add<GuidFormatter>()
                                         .Add<DateOnlyFormatter>()
                                         .Add<PrimitiveFormatter>();
        }

        public static FormatterBuilder Empty() => new FormatterBuilder();

    }

}
