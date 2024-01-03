using System;

using GenHTTP.Api.Protocol;

namespace GenHTTP.Modules.Reflection
{

    internal static class Adjustments
    {

        internal static IResponseBuilder Adjust(this IResponseBuilder builder, Action<IResponseBuilder>? adjustments)
        {
            if (adjustments != null)
            {
                adjustments(builder);
            }

            return builder;
        }

    }

}
