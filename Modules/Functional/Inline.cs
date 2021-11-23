using GenHTTP.Api.Protocol;
using GenHTTP.Modules.Functional.Provider;

namespace GenHTTP.Modules.Functional
{

    public static class Inline
    {

        public static InlineBuilder Create() => new();


        public static void Test()
        {
            Create().Any((IRequest r) => 1);
        }

    }

}
