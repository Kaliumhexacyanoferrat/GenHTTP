using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;

using GenHTTP.Api.Protocol;
using GenHTTP.Api.Content;

using GenHTTP.Modules.Conversion.Serializers.Forms;
using GenHTTP.Modules.Reflection;
using GenHTTP.Modules.Reflection.Operations;
using GenHTTP.Modules.IO;

public static class Invoker
{
    public static  ValueTask<IResponse?> Invoke(Delegate logic, Operation operation, IRequest request, IHandler handler, MethodRegistry registry)
    {
        var pathArgs = ArgumentProvider.MatchPath(operation, request);

        int? arg1 = null;

        if (request.Query.TryGetValue("i", out var queryArg1))
        {
            arg1 = (int?)registry.Formatting.Read(queryArg1, typeof(int));
        }

        var typedLogic = (Func<int?, int>)logic;

        var result = typedLogic(
            arg1 ?? default
        );

        var response = request.Respond()
                              .Content(registry.Formatting.Write(result, result.GetType()) ?? string.Empty)
                              .Build();

        return new(response);
    }
}
