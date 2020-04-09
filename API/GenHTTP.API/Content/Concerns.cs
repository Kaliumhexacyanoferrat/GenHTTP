using System;
using System.Collections.Generic;

namespace GenHTTP.Api.Content
{

    public static class Concerns
    {

        public static IHandler Chain(IHandler parent, IEnumerable<IConcernBuilder> concerns, Func<IHandler, IHandler> factory)
        {
            var stack = new Stack<IConcernBuilder>(concerns);

            return Chain(parent, stack, factory);
        }

        private static IHandler Chain(IHandler parent, Stack<IConcernBuilder> remainders, Func<IHandler, IHandler> factory)
        {
            if (remainders.Count > 0)
            {
                var concern = remainders.Pop();

                return concern.Build(parent, (parent) => Chain(parent, remainders, factory));
            }

            return factory(parent);
        }

    }

}
